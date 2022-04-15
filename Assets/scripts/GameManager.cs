using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using static managerHelper;
using static Analytics;

public class GameManager : MonoBehaviour {

    // Gives singleton behaviors to class
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }
    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    // Initial startup setup
    public GameObject startButtonPrefab;
    public GameObject canvas; // Holds the canvas of the screen

    // Prefabs for Game
    public GameObject cursorFollowerPrefab;
    public GameObject hoverPrefab;
    public GameObject printPrefab;
    public GameObject emptyImage;
    public GameObject textPrefab;
    public GameObject inventoryButtonPrefab;
    public GameObject layoutPrefab;
    public GameObject itemPrefab;
    public GameObject farmPrefab;
    public List<Sprite> itemSprites;

    // Game layout squares
    public GameObject inventory;
    public GameObject moneyBlock;
    public GameObject craftingInput;
    public GameObject square;
    public GameObject squareTrait;
    public GameObject craftingOutput;
    public GameObject swapView;
    public GameObject farm;

    // Cursor follower
    GameObject cursorFollower;
    GameObject hoverCursor;

    // Square variables
    int trait = 0; // 0 = quantity, 1 = grow rate, 2 = resistance
    
    // Item variables
    public List<GameObject> items;
    public List<GameObject> emptyInventory;
    public GameObject selectedItem;
    public const int INVENTORY_SIZE = 42;

    // Farm vars
    public List<GameObject> farms;
    public Sprite ownedSprite;
    public Sprite unownedSprite;
    public Sprite plantedSprite;
    public const int FARM_SIZE = 20;

    // Shop vars
    GameObject popup;
    int shopType; // shop types -> 0 = buy farm, 1 = buy greenhouse, 2 = buy random seed (maybe)
    int craftPrice = 100;
    const int FARM_PRICE = 100;
    const int GREENHOUSE_PRICE = 100;

    // Other
    [SerializeField] int money;
    int screenView;
    int turnNumber;

    private readonly (int, bool, int, bool)[] initSeeds = {
        (1, true, 1, false), // Bad trait
        (2, true, 2, false), // Badish trait
        (3, true, 1, false), // Good trait
        //(3, true, 2, false) // Great trait
    };


    // Start is called before the first frame update
    // This will set up the start screen (canvas + buttons)
    void Start()
    {
        GameObject button = Instantiate(startButtonPrefab);
        button.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().startGame(); } );
        button.transform.SetParent(canvas.transform, false);

        // Setup event system vars for raycasts
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = canvas.GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = canvas.GetComponent<EventSystem>();

        hoverCursor = Instantiate(hoverPrefab, hoverPrefab.transform.position, hoverPrefab.transform.rotation);
        hoverCursor.transform.SetParent(canvas.transform);
        hoverCursor.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        hoverCursor.SetActive(false);
        
    }

    [SerializeField] GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    [SerializeField] EventSystem m_EventSystem;

    private void Update() {
        // Do not do raycast if item not null
        hoverCursor.SetActive(false);
        if (selectedItem != null) {
            return;
        }

        m_PointerEventData = new PointerEventData(m_EventSystem);
        //Set the Pointer Event Position to that of the game object
        m_PointerEventData.position = Input.mousePosition;

        //Create a list of Raycast Results
        List<RaycastResult> results = new List<RaycastResult>();

        //Raycast using the Graphics Raycaster and mouse click position
        m_Raycaster.Raycast(m_PointerEventData, results);
        
        foreach (RaycastResult res in results) {
            if(res.gameObject.GetComponent<Button>()) {
                Transform resTran = res.gameObject.transform;
                if (resTran.parent.name == canvas.name) {
                    break;
                }
                // If gameObject is in inventory, craftingInput, or craftingOutput and has and item, display stats
                if (resTran.parent.name == inventory.name || resTran.parent.name == craftingInput.name || (resTran.parent.name == craftingOutput.name && resTran.name == "craftButton2")) {
                    if(resTran.childCount > 0) { // seed is contained
                        // 0-5 -> down; x % 6 -> right
                        int xOffset = 0;
                        int yOffset = 1;
                        if (resTran.parent.name == inventory.name) {
                            for(int i = 0; i < INVENTORY_SIZE; i++) {
                                Transform child = inventory.transform.GetChild(i);
                                if(resTran.name == child.name) {
                                    if(i < 6) {
                                        yOffset = -1;
                                    }
                                    if (i % 6 == 0) {
                                        xOffset = 2;
                                    }
                                    break;
                                }
                            }
                        }
                        (int,int,int) vals = resTran.GetChild(0).GetComponent<itemInfo>().getValues();
                        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        hoverCursor.SetActive(true);
                        string text = "R: " + vals.Item1 + " GR: " + vals.Item2 + " Q: " + vals.Item3;
                        hoverCursor.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
                        hoverCursor.GetComponent<RectTransform>().position = new Vector3(mousePos.x + xOffset, mousePos.y + yOffset, 0);
                        
                    }
                } else if(resTran.parent.name == farm.name) {
                    if (resTran.childCount > 1) { // seed is planted
                        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        hoverCursor.SetActive(true);
                        string text = "Grow time: " + resTran.GetChild(0).GetComponent<farmInfo>().growTime;
                        hoverCursor.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
                        hoverCursor.GetComponent<RectTransform>().position = new Vector3(mousePos.x, mousePos.y, 0);
                        
                    }
                }
                break;
            }
        }

    }

    //////////////////////////////////////////////
    ///
    /// INVENTORY BUTTON FUNCTIONS
    ///
    //////////////////////////////////////////////

    // Functionality on clicking inventory button
    public void inventoryButtonClick(GameObject obj) {
        if(popup != null) {
            Destroy(popup);
            popup = null;
            selectedItem = null;
            return;
        }

        if(selectedItem == null) { // Pick up item if there is one
            if (obj.transform.childCount != 0) {
                selectedItem = obj;
                GameObject child = obj.transform.GetChild(0).gameObject;
                cursorFollower = Instantiate(cursorFollowerPrefab, cursorFollowerPrefab.transform.position, cursorFollowerPrefab.transform.rotation);
                cursorFollower.GetComponent<SpriteRenderer>().sprite = child.GetComponent<Image>().sprite;
            } else { // Else pull up empty menu (only if in inventory)
                // TODO: Implement inventory store (buy seeds)
            }
        } else { // See if item can be put down
            if (obj.transform.childCount == 0) { // If empty put item down
                GameObject child = selectedItem.transform.GetChild(0).gameObject;
                child.transform.SetParent(obj.transform);
                child.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
                emptyInventory.Add(selectedItem);
                emptyInventory.Remove(obj);
                Destroy(cursorFollower);
                cursorFollower = null;
                selectedItem = null;
            } else if(obj.name != selectedItem.name) { // otherwise swaps places if it is a full space (that is not itself)
                GameObject child = selectedItem.transform.GetChild(0).gameObject;
                GameObject child2 = obj.transform.GetChild(0).gameObject;
                child.transform.SetParent(obj.transform);
                child2.transform.SetParent(selectedItem.transform);
                child.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
                child2.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
                Destroy(cursorFollower);
                cursorFollower = null;
                selectedItem = null;
            } else { // Drops item if reclick same place
                Destroy(cursorFollower);
                cursorFollower = null;
                selectedItem = null;
            }
        }
        updateSquare();
    }

    public void farmButtonClick(GameObject obj) {
        if(popup != null) {
            Destroy(popup);
            popup = null;
            selectedItem = null;
            return;
        }
        if (selectedItem == null) {
            if (obj.transform.GetChild(0).GetComponent<farmInfo>().plotOwned) { // Unowned
                if (!obj.transform.GetChild(0).GetComponent<farmInfo>().hasGreenhouse) {
                    popup = createPopup(inventoryButtonPrefab, canvas, textPrefab, "buy greenhouse $100");
                    popup.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().buyShopItem(); });
                    shopType = 1;
                    selectedItem = obj;
                } else {
                    Debug.Log("Greenhouse already owned"); // TODO: Replace with drifting text
                    createPrintText(printPrefab, canvas, "Greenhouse already owned.");
                }

            } else {
                popup = createPopup(inventoryButtonPrefab, canvas, textPrefab, "buy plot $100");
                popup.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().buyShopItem(); });
                shopType = 0;
                selectedItem = obj;
            }
        } else { // Plant seed
            if (obj.transform.childCount > 1) {
                // TODO: replace with popup
                Debug.Log("Seed already planted here");
                createPrintText(printPrefab, canvas, "Seed already planted here.");
            } else if (!obj.transform.GetChild(0).GetComponent<farmInfo>().plotOwned) {
                Debug.Log("You do not own this plot");
                createPrintText(printPrefab, canvas, "You do not own this plot.");
            } else { // Plants seed
                GameObject child = selectedItem.transform.GetChild(0).gameObject;
                child.transform.SetParent(obj.transform);
                child.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
                plantSeed(child, obj.transform.GetChild(0).gameObject);
                emptyInventory.Add(selectedItem);
                Destroy(cursorFollower);
                cursorFollower = null;
                selectedItem = null;
            }
        }
    }

    // Uses seed values to determine growth time
    public void plantSeed(GameObject seed, GameObject farm) {
        // (res, grow, quantity)
        (int, int, int) vals = seed.GetComponent<itemInfo>().getValues();

        // Grow time is calculated to be turns = 5 - grow stat (1 turn ~ 3 months)
        farm.GetComponent<farmInfo>().growTime = 5 - vals.Item2;
        farm.GetComponent<Image>().sprite = plantedSprite;
        seed.GetComponent<Image>().color = new Color(255,255,255,0);
    }

    public void buyShopItem() {
        if(shopType == 0) { // Buy farm plot
            if(money < FARM_PRICE) {
                Debug.Log("not enough money for purchase"); // TODO: replace with drifting text
                createPrintText(printPrefab, canvas, "Not enough money for purchase.");
            } else {
                setMoney(money - FARM_PRICE);
                selectedItem.transform.GetChild(0).GetComponent<Image>().sprite = ownedSprite;
                selectedItem.transform.GetChild(0).GetComponent<farmInfo>().plotOwned = true;
            }
        } else if(shopType == 1) { // Buy greenhouse on farm plot
            if (money < GREENHOUSE_PRICE) {
                Debug.Log("not enough money for purchase"); // TODO: replace with drifting text
                createPrintText(printPrefab, canvas, "Not enough money for purchase.");
            } else {
                setMoney(money - GREENHOUSE_PRICE);
                selectedItem.transform.GetChild(0).GetComponent<farmInfo>().hasGreenhouse = true;
                // TODO: Add greenhouse sprite
            }
        } else { // buy seed

        }
        Destroy(popup);
        popup = null;
        selectedItem = null;
    }

    public void setTrait(int value) {
        if(popup != null) {
            Destroy(popup);
            popup = null;
            selectedItem = null;
            return;
        } else if (selectedItem != null) {
            Destroy(cursorFollower);
            cursorFollower = null;
            selectedItem = null;
            return;
        }
        trait = value;
        updateSquare();
    }

    public void craftItem() {
        if (popup != null) {
            Destroy(popup);
            popup = null;
            selectedItem = null;
            return;
        } else if (selectedItem != null) {
            Destroy(cursorFollower);
            cursorFollower = null;
            selectedItem = null;
            return;
        }

        // Checks for inventory space
        if(!isInvSpace()) {
            return;
        }

        // Use seeds from parents
        Transform child0 = craftingInput.transform.GetChild(0);
        Transform child1 = craftingInput.transform.GetChild(1);
        if (child0.childCount == 0 || child1.childCount == 0) {
            Debug.Log("Two items needed to craft."); // TODO: replace with text bubble
            createPrintText(printPrefab, canvas, "Two items needed to craft.");
            } else if (craftingOutput.transform.GetChild(1).childCount != 0 || items.Count >= INVENTORY_SIZE) {
            Debug.Log("Crafted box full"); // TODO: replace with text bubble
            createPrintText(printPrefab, canvas, "Crafting box full.");
        } else {
            // Charges money for seeds
            if (money < craftPrice) {
                Debug.Log("Not enough money");
                createPrintText(printPrefab, canvas, "Not enough money.");
                return;
            } else {
                setMoney(money - craftPrice);
                craftPrice += 100;
            }

            GameObject item = setupItem(itemPrefab, craftingOutput.transform.GetChild(1).gameObject);
            item.GetComponent<Image>().sprite = itemSprites[Mathf.FloorToInt(Random.Range(0, itemSprites.Count)) % (itemSprites.Count)]; // TODO: Somehow set a sprite

            // Adds all of the needed elements from the parent (generation, quantity, grow rate, resistance)
            bool mutation = item.GetComponent<itemInfo>().createNewSeed(child0.GetChild(0).gameObject, child1.GetChild(0).gameObject);
            if (mutation) { // TODO: Edit mutation to print what changed when mutation occurs
                Debug.Log("A mutation occured"); // TODO: replace with something else to show a mutation occured
                createPrintText(printPrefab, canvas, "A mutation occured.");
            }
            items.Add(item);

            // Reports seed to analytics
            ReportCraft("p1", item.GetComponent<itemInfo>().getValues(), turnNumber, money);
        }
    }

    public bool isInvSpace() {
        if(INVENTORY_SIZE < items.Count) {
            Debug.Log("No additional items can be added. Inventory is full.");
            return false;
        }
        return true;
    }

    public void switchCraftView() {
        if (popup != null) {
            Destroy(popup);
            popup = null;
            selectedItem = null;
            return;
        } else if (selectedItem != null) {
            Destroy(cursorFollower);
            cursorFollower = null;
            selectedItem = null;
            return;
        }
        if (screenView == 0) {
            return;
        }
        screenView = 0;
        // Enable crafting view things
        craftingInput.SetActive(true);
        square.SetActive(true);
        squareTrait.SetActive(true);
        craftingOutput.SetActive(true);

    // Disable farm view
        farm.SetActive(false);
    }

    public void switchFarmView() {
        if (popup != null) {
            Destroy(popup);
            popup = null;
            selectedItem = null;
            return;
        } else if (selectedItem != null) {
            Destroy(cursorFollower);
            cursorFollower = null;
            selectedItem = null;
            return;
        }
        if (screenView == 1) {
            return;
        }
        screenView = 1;
        // Enable farm view
        farm.SetActive(true);

        // Disable crafting view things
        craftingInput.SetActive(false);
        square.SetActive(false);
        squareTrait.SetActive(false);
        craftingOutput.SetActive(false);
        
    }
   

    private void setMoney(int value) {
        money = value;
        moneyBlock.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "$" + money;
    }

    public void endTurn() {
        // TODO: Implement end of turn (prolly after making money)
        turnNumber += 1;

        // Report state to analytics
        int maxVal = 0;
        int numFarms = 0;
        foreach(GameObject seed in items) {
            (int, int, int) vals = seed.GetComponent<itemInfo>().getValues();
            if(maxVal < vals.Item1 + vals.Item2 + vals.Item3) {
                maxVal = vals.Item1 + vals.Item2 + vals.Item3;
            }
        }
        foreach(GameObject farmPlot in farms) {
            if(farmPlot.GetComponent<farmInfo>().plotOwned) {
                numFarms += 1;
            }
        }
        ReportPlayerState("p1", maxVal, turnNumber, money, numFarms);

        // TODO: Implement a screen transition which then shows the money made


        // Check/decrease farms counters. If any at 0, harvest and set seed back in inventory
        foreach(GameObject farmPlot in farms) {
            if(farmPlot.GetComponent<farmInfo>().plotOwned && farmPlot.GetComponent<farmInfo>().growTime != -1) {
                farmPlot.GetComponent<farmInfo>().growTime -= 1;
                if(farmPlot.GetComponent<farmInfo>().growTime == 0) {
                    // set growtime to -1 and reset sprite
                    farmPlot.GetComponent<farmInfo>().growTime = -1;
                    farmPlot.GetComponent<Image>().sprite = ownedSprite;

                    // Add money based on harvested crops
                    Transform child = farmPlot.transform.parent.GetChild(1);
                    (int,int,int) vals = child.GetComponent<itemInfo>().getValues();
                    setMoney(money + 100 * vals.Item3);

                    // Remove the seed and place into a spot
                    if(emptyInventory.Count == 0) {
                        Debug.Log("An inventory overflow error has occured");
                        return;
                    }
                    child.SetParent(emptyInventory[0].transform);
                    emptyInventory.RemoveAt(0);
                    child.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
                    child.GetComponent<Image>().color = new Color(255,255,255,255);
                }
                
            }
        }
        
    }

    // Show seeds that are in the table
    public void updateSquare() {
        // trait -> 0 = quantity, 1 = grow rate, 2 = resistance
        // child0 - elements on the side; child1 - elements on the top
        Transform child0 = craftingInput.transform.GetChild(0);
        Transform child1 = craftingInput.transform.GetChild(1);

        if (child0.childCount > 0 && child1.childCount > 0) { // Check squares, if both, populate whole table
            clearSquare();
            (int, bool, int, bool) vals0 = getItemInfo(child0.GetChild(0), trait);
            (int, bool, int, bool) vals1 = getItemInfo(child1.GetChild(0), trait);
            (string, string) strs0 = getReadableItemFormat(vals0, trait);
            (string, string) strs1 = getReadableItemFormat(vals1, trait);
            // Side vals
            square.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = strs1.Item1;
            square.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = strs1.Item2;
            square.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = strs0.Item1;
            square.transform.GetChild(6).GetChild(0).GetComponent<TextMeshProUGUI>().text = strs0.Item2;

            // Center table vals
            square.transform.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>().text = getItemsTable(vals0.Item1, vals0.Item2, vals1.Item1, vals1.Item2, trait);
            square.transform.GetChild(5).GetChild(0).GetComponent<TextMeshProUGUI>().text = getItemsTable(vals0.Item1, vals0.Item2, vals1.Item3, vals1.Item4, trait);
            square.transform.GetChild(7).GetChild(0).GetComponent<TextMeshProUGUI>().text = getItemsTable(vals0.Item3, vals0.Item4, vals1.Item1, vals1.Item2, trait);
            square.transform.GetChild(8).GetChild(0).GetComponent<TextMeshProUGUI>().text = getItemsTable(vals0.Item3, vals0.Item4, vals1.Item3, vals1.Item4, trait);

        } else if (child0.childCount > 0) { // If there is seed in space 1, then populate side
            clearSquare();
            (string, string) strs = getReadableItemFormat(getItemInfo(child0.GetChild(0), trait), trait);
            square.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = strs.Item1;
            square.transform.GetChild(6).GetChild(0).GetComponent<TextMeshProUGUI>().text = strs.Item2;


        } else if (child1.childCount > 0) { // If there is a seed int space 2, then populate top
            clearSquare();
            (string, string) strs = getReadableItemFormat(getItemInfo(child1.GetChild(0), trait), trait);
            square.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = strs.Item1;
            square.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = strs.Item2;

        } else { // Else, clear square
            clearSquare();
        }
    }

    public void clearSquare() {
        for (int i = 1; i < square.transform.childCount; i++) {
            square.transform.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        }
    }
    
    //////////////////////////////////////////////
    ///
    /// MAIN MENU FUNCTIONS
    ///
    //////////////////////////////////////////////


    public void startGame() {
        // Removes main menu
        List<Transform> children = new List<Transform>();
        foreach(Transform child in canvas.transform) {
            children.Add(child);
        }
        
        foreach(Transform child in children) {
            Destroy(child.gameObject);
        }

        screenView = 0;
        turnNumber = 1;

        // Add things to new scene
        buildGameScene();
    }

    private void buildGameScene() {
        // Instantiates canvas prefabs
        inventory = Instantiate(layoutPrefab); // inventory
        inventory.name = "inventory";
        inventory.transform.SetParent(canvas.transform);
        moneyBlock = Instantiate(layoutPrefab); // money
        moneyBlock.name = "moneyBlock";
        moneyBlock.transform.SetParent(canvas.transform);
        craftingInput = Instantiate(layoutPrefab); // craftingInput
        craftingInput.name = "craftingInput";
        craftingInput.transform.SetParent(canvas.transform);
        square = Instantiate(layoutPrefab); // Square
        square.name = "square";
        square.transform.SetParent(canvas.transform);
        squareTrait = Instantiate(layoutPrefab); // swapView
        squareTrait.name = "squareTrait";
        squareTrait.transform.SetParent(canvas.transform);
        craftingOutput = Instantiate(layoutPrefab); // craftingOutput
        craftingOutput.name = "craftingOutput";
        craftingOutput.transform.SetParent(canvas.transform);
        swapView = Instantiate(layoutPrefab); // swapView
        swapView.name = "swapView";
        swapView.transform.SetParent(canvas.transform);
        farm = Instantiate(layoutPrefab); // farm
        farm.name = "farm";
        farm.transform.SetParent(canvas.transform);

        // Does bounds setup for the layout groups
        int width = Camera.main.pixelWidth;
        int height = Camera.main.pixelHeight;
        Debug.Log(width + " " + height);

        // Inventory
        layoutSetup(inventory, 0, Mathf.FloorToInt(width * .5f), 0, 0); // Takes around 60% of the screen, otherwise 650px works
        // Crafting
        layoutSetup(craftingInput, Mathf.FloorToInt(width * .5f), 0, Mathf.FloorToInt(height * .1f), Mathf.FloorToInt(height * .75f));
        layoutSetup(square, Mathf.FloorToInt(width * .5f), Mathf.FloorToInt(width * .2f), Mathf.FloorToInt(height * .25f), Mathf.FloorToInt(height * .25f));
        layoutSetup(craftingOutput, Mathf.FloorToInt(width * .5f), 0, Mathf.FloorToInt(height * .75f), Mathf.FloorToInt(height * .1f));
        layoutSetup(squareTrait, Mathf.FloorToInt(width * .8f), 0, Mathf.FloorToInt(height * .25f), Mathf.FloorToInt(height * .25f));
        // Swap View
        layoutSetup(swapView, Mathf.FloorToInt(width * .5f), 0, Mathf.FloorToInt(height * .9f), 0);
        // Money
        layoutSetup(moneyBlock, Mathf.FloorToInt(width * .5f), 0, 0, Mathf.FloorToInt(height * .9f));
        // Farm
        layoutSetup(farm, Mathf.FloorToInt(width * .5f), 0, Mathf.FloorToInt(height * .1f), Mathf.FloorToInt(height * .1f));

        // Adds additional constraints
        // inventory cell size -> x=160, y=154
        inventory.GetComponent<GridLayoutGroup>().cellSize = new Vector2(160, 154);
        moneyBlock.GetComponent<GridLayoutGroup>().cellSize = new Vector2(width/2, Mathf.FloorToInt(height * .1f));
        craftingInput.GetComponent<GridLayoutGroup>().cellSize = new Vector2(160, 154);
        square.GetComponent<GridLayoutGroup>().cellSize = new Vector2(160, 154);
        square.GetComponent<GridLayoutGroup>().constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        square.GetComponent<GridLayoutGroup>().constraintCount = 3;
        squareTrait.GetComponent<GridLayoutGroup>().cellSize = new Vector2(160, 154);
        craftingOutput.GetComponent<GridLayoutGroup>().cellSize = new Vector2(160, 154);
        swapView.GetComponent<GridLayoutGroup>().cellSize = new Vector2(width/6, Mathf.FloorToInt(height * .1f));
        farm.GetComponent<GridLayoutGroup>().cellSize = new Vector2(160, 154);

        // Sets up buttons in inventory
        Debug.Log("Finished Layout setup");
        inventorySetup();
        Debug.Log("Finished Inventory Setup");
        moneySetup();
        Debug.Log("Finished Money Setup");
        craftingSetup();
        Debug.Log("Finished Crafting Setup");
        swapViewSetup();
        Debug.Log("Finished Swap View Setup");
        farmSetup();

        hoverCursor = Instantiate(hoverPrefab, hoverPrefab.transform.position, hoverPrefab.transform.rotation);
        hoverCursor.transform.SetParent(canvas.transform);
        hoverCursor.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        hoverCursor.SetActive(false);
    }

    // Sets up the inventory spaces
    private void inventorySetup() {
        for (int i = 0; i < INVENTORY_SIZE; i++) {
            // TODO: Either make trash button, or add new button for trash/sell (maybe sell for a tiny amount)
            GameObject button = setupButton(inventoryButtonPrefab, inventory); // sets the parent to the inventory and the localScale to 1 (so it's not huge when it's made)
            button.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().inventoryButtonClick(button); });
            
            button.name = "button" + i;

            // Build initial items to use and adds meta data
            if (i < 4) {
                GameObject item = setupItem(itemPrefab, button);
                item.GetComponent<Image>().sprite = itemSprites[i % (itemSprites.Count)];
                item.GetComponent<itemInfo>().generation = 0;
                item.GetComponent<itemInfo>().setResistance(initSeeds[i % initSeeds.Length].Item1, initSeeds[i % initSeeds.Length].Item2, initSeeds[i % initSeeds.Length].Item3, initSeeds[i % initSeeds.Length].Item4);
                item.GetComponent<itemInfo>().setGrowthRate(initSeeds[(i + 1) % initSeeds.Length].Item1, initSeeds[(i + 1) % initSeeds.Length].Item2, initSeeds[(i + 1) % initSeeds.Length].Item3, initSeeds[(i + 1) % initSeeds.Length].Item4);
                item.GetComponent<itemInfo>().setQuantity(initSeeds[(i + 2) % initSeeds.Length].Item1, initSeeds[(i + 2) % initSeeds.Length].Item2, initSeeds[(i + 2) % initSeeds.Length].Item3, initSeeds[(i + 2) % initSeeds.Length].Item4);
                
                items.Add(item);
            } else {
                emptyInventory.Add(button);
            }
        }
    }
    
    private void moneySetup() {
        GameObject button = setupButton(inventoryButtonPrefab, moneyBlock); // sets the parent to the inventory and the localScale to 1 (so it's not huge when it's made)
        button.name = "money";
        money = 400;
        GameObject moneyText = setupText(textPrefab, button, "$"+money);
    }

    // Sets up each part of the crafting menu
    // inventory;
    // craftingInput;
    // square;
    // craftingOutput;
    private void craftingSetup() {
        // Sets up the items used to craft
        GameObject craftButton = setupButton(inventoryButtonPrefab, craftingInput); // sets the parent to the inventory and the localScale to 1 (so it's not huge when it's made)
        craftButton.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().inventoryButtonClick(craftButton); });
        craftButton.name = "craftButton0";
        GameObject craftButton1 = setupButton(inventoryButtonPrefab, craftingInput); // sets the parent to the inventory and the localScale to 1 (so it's not huge when it's made)
        craftButton1.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().inventoryButtonClick(craftButton1); });
        craftButton1.name = "craftButton1";

        // Sets up punnet square (make empty spaces squares as well (maybe make 3 punnet squares?))
        GameObject emptySpace = setupButton(emptyImage, square); // Set up empty space
        GameObject squareButton0 = setupButton(inventoryButtonPrefab, square); // Sets up first row buttons
        GameObject squareButText0 = setupText(textPrefab, squareButton0, "zero");
        GameObject squareButton1 = setupButton(inventoryButtonPrefab, square);
        GameObject squareButText1 = setupText(textPrefab, squareButton1, "one");
        GameObject squareButton2 = setupButton(inventoryButtonPrefab, square); // Sets up second row buttons
        GameObject squareButText2 = setupText(textPrefab, squareButton2, "two");
        GameObject square0 = setupButton(inventoryButtonPrefab, square);
        GameObject squareText0 = setupText(textPrefab, square0, "0");
        GameObject square1 = setupButton(inventoryButtonPrefab, square);
        GameObject squareText1 = setupText(textPrefab, square1, "1");
        GameObject squareButton3 = setupButton(inventoryButtonPrefab, square); // Sets up thrid row buttons
        GameObject squareButText3 = setupText(textPrefab, squareButton3, "three");
        GameObject square2 = setupButton(inventoryButtonPrefab, square);
        GameObject squareText2 = setupText(textPrefab, square2, "2");
        GameObject square3 = setupButton(inventoryButtonPrefab, square);
        GameObject squareText3 = setupText(textPrefab, square3, "3");

        // Sets up the buttons to swap traits
        GameObject traitButton0 = setupButton(inventoryButtonPrefab, squareTrait);
        GameObject traitButText0 = setupText(textPrefab, traitButton0, "Quantity");
        traitButton0.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().setTrait(0); });
        GameObject traitButton1 = setupButton(inventoryButtonPrefab, squareTrait);
        GameObject traitButText1 = setupText(textPrefab, traitButton1, "Grow Rate");
        traitButton1.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().setTrait(1); });
        GameObject traitButton2 = setupButton(inventoryButtonPrefab, squareTrait);
        GameObject traitButText2 = setupText(textPrefab, traitButton2, "Resistance");
        traitButton2.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().setTrait(2); });

        // Sets up craft button and square where crafted items are placed
        GameObject doCraftButton = setupButton(inventoryButtonPrefab, craftingOutput); // sets the parent to the inventory and the localScale to 1 (so it's not huge when it's made)
        doCraftButton.name = "doCraftButton";
        GameObject doCraftText1 = setupText(textPrefab, doCraftButton, "Craft Seed");
        doCraftButton.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().craftItem(); });

        GameObject craftButton2 = setupButton(inventoryButtonPrefab, craftingOutput); // sets the parent to the inventory and the localScale to 1 (so it's not huge when it's made)
        craftButton2.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().inventoryButtonClick(craftButton2); });
        craftButton2.name = "craftButton2";
    }

    private void swapViewSetup() {
        GameObject craftingView = setupButton(inventoryButtonPrefab, swapView); // sets the parent to the inventory and the localScale to 1 (so it's not huge when it's made)
        craftingView.name = "craftingView";
        GameObject craftingViewText = setupText(textPrefab, craftingView, "Crafting View");
        craftingView.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().switchCraftView(); });
        // Here add functionality
        GameObject farmInventoryButton = setupButton(inventoryButtonPrefab, swapView); // sets the parent to the inventory and the localScale to 1 (so it's not huge when it's made)
        farmInventoryButton.name = "farmViewButton";
        GameObject farmInventoryText = setupText(textPrefab, farmInventoryButton, "Farm View");
        farmInventoryButton.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().switchFarmView(); });
        // Here add functionality
        GameObject endTurnButton = setupButton(inventoryButtonPrefab, swapView); // sets the parent to the inventory and the localScale to 1 (so it's not huge when it's made)
        endTurnButton.name = "endTurnButton";
        GameObject endTurnText = setupText(textPrefab, endTurnButton, "End Turn");
        // Here add functionality
        endTurnButton.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().endTurn(); });
        
    }

    // sets up the farm plots (yours and opponents) and hides it
    private void farmSetup() {
        for (int i = 0; i < FARM_SIZE; i++) {
            // Maybe make farms their own button
            GameObject button = setupButton(inventoryButtonPrefab, farm); // sets the parent to the inventory and the localScale to 1 (so it's not huge when it's made)
            button.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().farmButtonClick(button); });
            // TODO: Add new sprites to farms to indicate unbought, bought, plated, greenhouse, etc.
            button.name = "farm" + i;

            // Builds initial plots and add their meta data
            GameObject farmPlot = setupItem(farmPrefab, button);
            if (i < 4) { // owned plots
                farmPlot.GetComponent<Image>().sprite = ownedSprite;
                farmPlot.GetComponent<farmInfo>().plotOwned = true;
            } else { // Unowned plots
                farmPlot.GetComponent<Image>().sprite = unownedSprite;
                farmPlot.GetComponent<farmInfo>().plotOwned = false;
            }
            farmPlot.GetComponent<farmInfo>().hasGreenhouse = false;
            farmPlot.GetComponent<farmInfo>().growTime = -1;
            farms.Add(farmPlot);
        }
        farm.SetActive(false);
    }

    // Sets up the store (the store doesn't need setup. It will be a like a button)
    // fertilizer
    // greenhouse
    // new seeds?
    private void storeSetup() {
        
    }
}
