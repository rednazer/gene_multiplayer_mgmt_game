using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;

using static managerHelper;
using static Analytics;
using System.Text;

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
    public GameObject clientConnectPrefab;
    public GameObject canvas; // Holds the canvas of the screen
    static public NetworkObject networkPlayer;
    private bool atMainMenu;

    // Prefabs for Game
    public GameObject cursorFollowerPrefab;
    public GameObject hoverPrefab;
    public GameObject printPrefab;
    public GameObject emptyImage;
    public GameObject textPrefab;
    public GameObject inventoryButtonPrefab;
    public GameObject layoutPrefab;
    public GameObject itemPrefab;
    public GameObject contractPrefab;
    public GameObject flowerPrefab;
    public GameObject farmPrefab;
    public List<Sprite> itemSprites;
    public List<Sprite> flowerSprites;

    // Game layout squares
    public GameObject inventory;
    public GameObject moneyBlock;
    public GameObject craftingInput;
    public GameObject square;
    public GameObject squareTrait;
    public GameObject craftingOutput;
    public GameObject swapView;
    public GameObject farm;
    public GameObject contracts;

    // Cursor follower
    GameObject cursorFollower;
    GameObject hoverCursor;

    // Square variables
    int trait = 0; // 0 = color, 1 = height, 2 = fast/plentiful

    // Item variables
    public List<GameObject> items;
    public List<GameObject> emptyInventory;
    public GameObject selectedItem;
    public const int INVENTORY_SIZE = 42;

    // Farm vars
    public List<GameObject> farms;
    public int plantedItems = 0;
    public Sprite farmBackground;
    public Sprite ownedSprite;
    public Sprite unownedSprite;
    public Sprite plantedSprite;
    public const int FARM_SIZE = 15;

    // Shop vars
    GameObject popup;
    int shopType; // shop types -> 0 = buy farm
    int craftPrice = BASE_CRAFT_PRICE;
    const int INIT_MONEY = 200;
    const int BASE_CRAFT_PRICE = 0;
    const int CRAFT_PRICE_INCREASE = 50;
    const int FARM_PRICE = 100;

    // Other
    [SerializeField] int money;
    int screenView;
    [SerializeField] int turnNumber;
    GameObject endTurnBut;
    public bool debugMode;
    int seedsCrafted=0;

    // QOL highlight/press colors
    public ColorBlock buttonColors;

    private readonly (int, int)[] initSeeds = {
        (0, 0), // dominant traits
        (0, 1), // mixed traits
        (1, 1), // non-dominant traits
    };


    // Start is called before the first frame update
    // This will set up the start screen (canvas + buttons)
    void Start() {
        if (debugMode) {
            GameObject button = Instantiate(startButtonPrefab);
            button.name = "start";
            button.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().startGame(); });
            button.transform.SetParent(canvas.transform, false);
            button.GetComponent<Button>().transition = Selectable.Transition.ColorTint;
            button.GetComponent<Button>().colors = buttonColors;


            // Tests making a log file and writing to it
            ReportPlayerState("0", 0, 0, 0, 0);
        }
        GameObject hostButton = Instantiate(startButtonPrefab);
        hostButton.name = "hostButton";
        hostButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Host game";
        hostButton.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().hostGame(); });
        hostButton.transform.SetParent(canvas.transform, false);
        hostButton.transform.position = new Vector3(0, .5f);
        hostButton.GetComponent<Button>().transition = Selectable.Transition.ColorTint;
        hostButton.GetComponent<Button>().colors = buttonColors;

        GameObject clientConnect = Instantiate(clientConnectPrefab);
        clientConnect.transform.SetParent(canvas.transform, false);
        clientConnect.transform.position = new Vector3(0, -1f);

        GameObject clientButton = Instantiate(startButtonPrefab);
        clientButton.name = "clientButton";
        clientButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Join game";
        clientButton.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().clientGame(clientConnect); });
        clientButton.transform.SetParent(canvas.transform, false);
        clientButton.transform.position = new Vector3(0, -.5f);
        clientButton.GetComponent<Button>().transition = Selectable.Transition.ColorTint;
        clientButton.GetComponent<Button>().colors = buttonColors;



        // Setup event system vars for raycasts
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = canvas.GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = canvas.GetComponent<EventSystem>();

        hoverCursor = Instantiate(hoverPrefab, hoverPrefab.transform.position, hoverPrefab.transform.rotation);
        hoverCursor.transform.SetParent(canvas.transform);
        hoverCursor.name = "hoverCursor";
        hoverCursor.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        hoverCursor.SetActive(false);
        atMainMenu = true;
    }

    void hostGame() {
        NetworkManager.Singleton.StartHost();
        networkPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        Debug.Log("host");
    }
    

    void clientGame(GameObject clientConnect) {
        Debug.Log("join");
        string text = clientConnect.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text;
        if(text == "") {
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = "127.0.0.1";
        } else {
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = text;
        }
        //NetworkManager.Singleton.StartClient();
    }

    [SerializeField] GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    [SerializeField] EventSystem m_EventSystem;
    /*
    private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback) {
        bool approval = false;
        bool createPlayerObj = false;
        if (atMainMenu) {
            approval = true;
            createPlayerObj = true;
        }

        callback(createPlayerObj, null, approval, null, null);
    }*/

   private void StartLocalGame() {
        Debug.Log("starting game");
        var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        var player = playerObject.GetComponent<networkScript>();
        player.TellStartGame();
        startGame();
    }

    private float timer = 5;

    private void Update() {

        if (!atMainMenu && NetworkManager.Singleton.IsServer) {
            tryEndTurn();
            timer -= Time.deltaTime;
            if (timer < 0) {
                checkContractUpdate();
            }
        } else if(!atMainMenu) {
            GameObject hostObj = GameObject.Find("networkPlayerHost");
            if (NetworkManager.Singleton.IsConnectedClient) {
                if (hostObj.GetComponent<networkScript>().turnNum.Value > turnNumber) {
                    endTurn();
                }
            }
            // also updates contracts
            checkContractUpdate();
        }

        // Do not do raycast if item not null
        hoverCursor.SetActive(false);
        if (selectedItem != null) {
            return;
        }
        if (atMainMenu && NetworkManager.Singleton.IsServer) {
            if (NetworkManager.Singleton.ConnectedClientsIds.Count == 2) {
                StartLocalGame();
            }
        } else if (atMainMenu && NetworkManager.Singleton.IsClient) {
            if (NetworkManager.Singleton.IsConnectedClient) {
                GameObject hostObj = GameObject.Find("networkPlayerHost");
                if (hostObj.GetComponent<networkScript>().IsGameStarted()) {
                    StartLocalGame();
                }
            }
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
                                        xOffset = 3;
                                    } else if(i % 6 == 1) {
                                        xOffset = 1;
                                    }
                                    break;
                                }
                            }
                        }
                        if (resTran.GetChild(0).GetComponent<itemInfo>()) {
                            (string, string, string) vals = resTran.GetChild(0).GetComponent<itemInfo>().getStrings();
                            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            hoverCursor.SetActive(true); // TODO: fix formatting of popup text
                            string text = "Color: " + vals.Item1 + "\nHeight: " + vals.Item2 + "\nQuality: " + vals.Item3;
                            hoverCursor.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
                            hoverCursor.GetComponent<RectTransform>().position = new Vector3(mousePos.x + xOffset, mousePos.y + yOffset, 0);
                        } else if(resTran.GetChild(0).GetComponent<flowerInfo>()) {
                            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            hoverCursor.SetActive(true); // TODO: fix formatting of popup text
                            string text = resTran.GetChild(0).GetComponent<flowerInfo>().itemInfo();
                            hoverCursor.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
                            hoverCursor.GetComponent<RectTransform>().position = new Vector3(mousePos.x + xOffset, mousePos.y + yOffset, 0);
                        }
                        
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
                // Removed Greenhouse as a shop
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
                if (!isInvSpace()) {
                    return;
                }
                plantedItems += 1;
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
        int growSpeed = seed.GetComponent<itemInfo>().growSpeed();

        // Grow time is calculated to be turns = 5 - grow stat (1 turn ~ 3 months)
        farm.GetComponent<farmInfo>().growTime = growSpeed;
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
            // removed greenhouse as a shop item
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

    public void craftItem(GameObject obj) {
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
            } else if (craftingOutput.transform.GetChild(1).childCount != 0 || items.Count + plantedItems >= INVENTORY_SIZE) {
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
                craftPrice += CRAFT_PRICE_INCREASE;
                obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText("Craft Seed for $" +craftPrice);
            }

            GameObject item = setupItem(itemPrefab, craftingOutput.transform.GetChild(1).gameObject);
            item.GetComponent<Image>().sprite = itemSprites[Mathf.FloorToInt(Random.Range(0, itemSprites.Count)) % (itemSprites.Count)]; // TODO: Somehow set a sprite

            // Adds all of the needed elements from the parent (generation, quantity, grow rate, resistance)
            item.GetComponent<itemInfo>().createNewSeed(child0.GetChild(0).gameObject, child1.GetChild(0).gameObject);
            seedsCrafted++;
            (string, string, string) vals = item.GetComponent<itemInfo>().getStrings();
            string printStr = "A " + vals.Item1 + " " + vals.Item2 + " flower was made with " + vals.Item3 + ".";
            //Debug.Log(printStr); // TODO: replace with something else to show a mutation occured
            createPrintText(printPrefab, canvas, printStr);
            items.Add(item);

            // Reports seed to analytics
            itemInfo iInfo = item.GetComponent<itemInfo>();
            (int, int, int) values = item.GetComponent<itemInfo>().getValues();

            if (NetworkManager.Singleton.IsServer) {
                ReportCraft(""+NetworkManager.Singleton.LocalClientId, values, seedsCrafted, turnNumber, money);
            }
        }
    }

    public void sellItem(GameObject item) {
        if (popup != null) {
            Destroy(popup);
            popup = null;
            selectedItem = null;
            return;
        }
        if(selectedItem == null) {
            Debug.Log("Drag an item to sell.");
            createPrintText(printPrefab, canvas, "Drag an item to sell.");
        } else {
            // Checks if for selling seeds
            if (item.transform.GetChild(0).GetComponent<contractInfo>().buySeed) {
                if (selectedItem.transform.GetChild(0).GetComponent<itemInfo>()) {
                    GameObject child = selectedItem.transform.GetChild(0).gameObject;
                    emptyInventory.Add(selectedItem);
                    selectedItem.transform.DetachChildren();
                    Destroy(cursorFollower);
                    Destroy(child);
                    selectedItem = null;
                    setMoney(money + item.transform.GetChild(0).GetComponent<contractInfo>().price);
                } else {
                    Debug.Log("this is a flower, not a seed");
                    createPrintText(printPrefab, canvas, "this is a flower, not a seed");
                }
            } else { // selling for flowers
                if (selectedItem.transform.GetChild(0).GetComponent<flowerInfo>()) {
                    // get contract and flower info
                    contractInfo cInfo = item.transform.GetChild(0).GetComponent<contractInfo>();
                    flowerInfo fInfo = selectedItem.transform.GetChild(0).GetComponent<flowerInfo>();

                    // Checks for generic flower sell
                    if(cInfo.remaining == -1) {
                        GameObject child = selectedItem.transform.GetChild(0).gameObject;
                        fInfo.numRemaining -= 1;
                        if (fInfo.numRemaining <= 0) {
                            emptyInventory.Add(selectedItem);
                            selectedItem.transform.DetachChildren();
                            Destroy(cursorFollower);
                            Destroy(child);
                            selectedItem = null;
                        }
                        setMoney(money + item.transform.GetChild(0).GetComponent<contractInfo>().price);
                        return;
                    }

                    // Does a little error checking
                    if (cInfo.remaining < 1) {
                        Debug.Log("Desync occured. Aborting");
                        return;
                    }

                    bool correctFlower = false;

                    // check if contract info matches flower info
                    if (cInfo.flowerHeight == -1) {
                        correctFlower = (cInfo.petalColor == fInfo.petalColor);
                    } else if(cInfo.growSpeed == -1) {
                        correctFlower = (cInfo.petalColor == fInfo.petalColor) && (cInfo.flowerHeight == fInfo.flowerHeight);
                    } else {
                        correctFlower = (cInfo.growSpeed == fInfo.growSpeed) && (cInfo.petalColor == fInfo.petalColor) && (cInfo.flowerHeight == fInfo.flowerHeight);
                    }

                    if(!correctFlower) {
                        Debug.Log("Flower does not meet requirements");
                        createPrintText(printPrefab, canvas, "Flower does not meet requirements");
                        return;
                    }


                    // Removes 1 of the flowers
                    fInfo.numRemaining -= 1;
                    cInfo.remaining -= 1;
                    setMoney(money + item.transform.GetChild(0).GetComponent<contractInfo>().price);
                    GameObject child2 = selectedItem.transform.GetChild(0).gameObject;
                    if (fInfo.numRemaining <= 0) {
                        emptyInventory.Add(selectedItem);
                        selectedItem.transform.DetachChildren();
                        Destroy(cursorFollower);
                        Destroy(child2);
                        selectedItem = null;
                    }

                    // Tries to sell the flower
                    sellFlower(item);

                    // Checks if contract number is 0, then creates a new contract
                    checkContractUpdate();
                }
            }
        }
    }

    private bool soldThisTurn;
    private float soldTimer = .4f;

    private void sellFlower(GameObject item) {
        // Decrements the contract/makes new contract is empty
        if (NetworkManager.Singleton.IsServer) {
            var obj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            obj.GetComponent<networkScript>().BuyFlower(item.transform.GetChild(0).GetComponent<contractInfo>().number);
            item.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = setContractText(item.transform.GetChild(0).GetComponent<contractInfo>());
        } else {
            // If not the server, sends a BuyFlower request to the server
            var obj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            obj.GetComponent<networkScript>().BuyFlower(item.transform.GetChild(0).GetComponent<contractInfo>().number);
            Debug.Log("sellflower");
            soldThisTurn = true;
            soldTimer = .4f;
        }
    }

    public void checkContractUpdate() {
        networkScript client = GameObject.Find("networkPlayerClient").GetComponent<networkScript>();
        networkScript host = GameObject.Find("networkPlayerHost").GetComponent<networkScript>();
        if (NetworkManager.Singleton.IsServer) {
            // Checks if client bought a flower
            if (client.contract1Remaining.Value < host.contract1Remaining.Value) {
                host.contract1Remaining.Value -= 1;
                contracts.transform.GetChild(2).GetChild(0).GetComponent<contractInfo>().remaining -= 1;
                contracts.transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>().text = setContractText(contracts.transform.GetChild(2).GetChild(0).GetComponent<contractInfo>());
            } else if(client.contract2Remaining.Value < host.contract2Remaining.Value) {
                host.contract2Remaining.Value -= 1;
                contracts.transform.GetChild(3).GetChild(0).GetComponent<contractInfo>().remaining -= 1;
                contracts.transform.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text = setContractText(contracts.transform.GetChild(3).GetChild(0).GetComponent<contractInfo>());
            } else if(client.contract3Remaining.Value < host.contract3Remaining.Value) {
                host.contract2Remaining.Value -= 1;
                contracts.transform.GetChild(4).GetChild(0).GetComponent<contractInfo>().remaining -= 1;
                
                contracts.transform.GetChild(4).GetChild(1).GetComponent<TextMeshProUGUI>().text = setContractText(contracts.transform.GetChild(4).GetChild(0).GetComponent<contractInfo>());
            }
            if (host.contract1Remaining.Value <= 0) {
                // Create new contract (for contract 1)
                createNewContract(1);
            }
            if (host.contract2Remaining.Value <= 0) {
                // Create new contract (for contract 2)
                createNewContract(2);
            }
            if (host.contract3Remaining.Value <= 0) {
                // Create new contract (for contract 3)
                createNewContract(3);
            }
        } else {
            // Update client network info
            if (!soldThisTurn) {
                client.SetContractInfoServerRpc(1, host.contract1Remaining.Value, host.contract1Price.Value, host.contract1PetalColor.Value, host.contract1FlowerHeight.Value, host.contract1GrowSpeed.Value);
                client.SetContractInfoServerRpc(2, host.contract2Remaining.Value, host.contract2Price.Value, host.contract2PetalColor.Value, host.contract2FlowerHeight.Value, host.contract2GrowSpeed.Value);
                client.SetContractInfoServerRpc(3, host.contract3Remaining.Value, host.contract3Price.Value, host.contract3PetalColor.Value, host.contract3FlowerHeight.Value, host.contract3GrowSpeed.Value);
            } else {
                soldTimer -= Time.deltaTime;
                if(soldTimer < 0) { 
                    soldThisTurn = false;
                }
                Debug.Log("did not update this turn");
            }
            // Update in-game info
            networkScript hostScript = GameObject.Find("networkPlayerHost").GetComponent<networkScript>();
            Transform sellContract = contracts.transform.GetChild(2);
            sellContract.GetChild(0).GetComponent<contractInfo>().petalColor = hostScript.contract1PetalColor.Value;
            sellContract.GetChild(0).GetComponent<contractInfo>().flowerHeight = hostScript.contract1FlowerHeight.Value;
            sellContract.GetChild(0).GetComponent<contractInfo>().growSpeed = hostScript.contract1GrowSpeed.Value;
            sellContract.GetChild(0).GetComponent<contractInfo>().price = hostScript.contract1Price.Value;
            sellContract.GetChild(0).GetComponent<contractInfo>().remaining = hostScript.contract1Remaining.Value;
            sellContract.GetChild(1).GetComponent<TextMeshProUGUI>().text = setContractText(sellContract.GetChild(0).GetComponent<contractInfo>());

            sellContract = contracts.transform.GetChild(3);
            sellContract.GetChild(0).GetComponent<contractInfo>().petalColor = hostScript.contract2PetalColor.Value;
            sellContract.GetChild(0).GetComponent<contractInfo>().flowerHeight = hostScript.contract2FlowerHeight.Value;
            sellContract.GetChild(0).GetComponent<contractInfo>().growSpeed = hostScript.contract2GrowSpeed.Value;
            sellContract.GetChild(0).GetComponent<contractInfo>().price = hostScript.contract2Price.Value;
            sellContract.GetChild(0).GetComponent<contractInfo>().remaining = hostScript.contract2Remaining.Value;
            sellContract.GetChild(1).GetComponent<TextMeshProUGUI>().text = setContractText(sellContract.GetChild(0).GetComponent<contractInfo>());

            sellContract = contracts.transform.GetChild(4);
            sellContract.GetChild(0).GetComponent<contractInfo>().petalColor = hostScript.contract3PetalColor.Value;
            sellContract.GetChild(0).GetComponent<contractInfo>().flowerHeight = hostScript.contract3FlowerHeight.Value;
            sellContract.GetChild(0).GetComponent<contractInfo>().growSpeed = hostScript.contract3GrowSpeed.Value;
            sellContract.GetChild(0).GetComponent<contractInfo>().price = hostScript.contract3Price.Value;
            sellContract.GetChild(0).GetComponent<contractInfo>().remaining = hostScript.contract3Remaining.Value;
            sellContract.GetChild(1).GetComponent<TextMeshProUGUI>().text = setContractText(sellContract.GetChild(0).GetComponent<contractInfo>());

        }
    }

    // Only the host should ever use this method
    public void createNewContract(int contractNumber) {
        contractInfo info = contracts.transform.GetChild(contractNumber + 1).GetChild(0).GetComponent<contractInfo>();
        if (contractNumber == 1) {
            info.GetComponent<contractInfo>().petalColor = Mathf.FloorToInt(Random.Range(0, 3));
            info.GetComponent<contractInfo>().flowerHeight = -1;
            info.GetComponent<contractInfo>().growSpeed = -1;
            info.GetComponent<contractInfo>().price = Mathf.FloorToInt(Random.Range(75, 125));
            info.GetComponent<contractInfo>().remaining = Mathf.FloorToInt(Random.Range(1, 6));
        } else if(contractNumber == 2) {
            info.GetComponent<contractInfo>().petalColor = Mathf.FloorToInt(Random.Range(0, 3));
            info.GetComponent<contractInfo>().flowerHeight = Mathf.FloorToInt(Random.Range(0, 2));
            info.GetComponent<contractInfo>().growSpeed = -1;
            info.GetComponent<contractInfo>().price = Mathf.FloorToInt(Random.Range(150, 225));
            info.GetComponent<contractInfo>().remaining = Mathf.FloorToInt(Random.Range(1, 6));
        } else {
            info.GetComponent<contractInfo>().petalColor = Mathf.FloorToInt(Random.Range(0, 3));
            info.GetComponent<contractInfo>().flowerHeight = Mathf.FloorToInt(Random.Range(0, 2));
            info.GetComponent<contractInfo>().growSpeed = Mathf.FloorToInt(Random.Range(0, 3));
            int speed = info.GetComponent<contractInfo>().growSpeed;
            if (speed == 0) { // fast
                info.GetComponent<contractInfo>().price = Mathf.FloorToInt(Random.Range(800, 1000));
                info.GetComponent<contractInfo>().remaining = Mathf.FloorToInt(Random.Range(1, 2));
            } else if (speed == 1) {
                info.GetComponent<contractInfo>().price = Mathf.FloorToInt(Random.Range(250, 350));
                info.GetComponent<contractInfo>().remaining = Mathf.FloorToInt(Random.Range(3, 7));
            } else { // high quantity
                info.GetComponent<contractInfo>().price = Mathf.FloorToInt(Random.Range(225, 275));
                info.GetComponent<contractInfo>().remaining = Mathf.FloorToInt(Random.Range(5, 10));
            }
        }
        Transform sellText = contracts.transform.GetChild(contractNumber + 1).GetChild(1);
        sellText.GetComponent<TextMeshProUGUI>().text = setContractText(info);

        // Sets contract info
        var playerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        playerObj.GetComponent<networkScript>().SetContractInfo(contractNumber, info);
        GameObject.Find("networkPlayerClient").GetComponent<networkScript>().SetContractInfo(contractNumber, info);
    }

    public bool isInvSpace() {

        if(INVENTORY_SIZE < items.Count + plantedItems) {
            Debug.Log("No additional items can be added. Inventory is full.");
            createPrintText(printPrefab, canvas, "No additional items can be added. Inventory is full.");
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

        // Disable contract view
        contracts.SetActive(false);
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

        // Disable contract view
        contracts.SetActive(false);
    }
   

    private void setMoney(int value) {
        money = value;
        moneyBlock.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "$" + money;
    }

    private void switchContractView() {
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
        if (screenView == 2) {
            return;
        }
        screenView = 2;

        // Enable contract view
        contracts.SetActive(true);

        // Disable crafting
        craftingInput.SetActive(false);
        square.SetActive(false);
        squareTrait.SetActive(false);
        craftingOutput.SetActive(false);

        // Disable farm view
        farm.SetActive(false);
    }

    public void ReadyEndTurn(GameObject obj) {
        NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<networkScript>().ReadyEndTurn();
        obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Waiting for other player";
    }

    public void tryEndTurn() {
        GameObject server = GameObject.Find("networkPlayerHost");
        GameObject client = GameObject.Find("networkPlayerClient");
        // Checks if both have readied up for end turn
        if (server.GetComponent<networkScript>().endTurn.Value && client.GetComponent<networkScript>().endTurn.Value) {
            server.GetComponent<networkScript>().EndTurn(turnNumber+1);
            server.GetComponent<networkScript>().endTurn.Value = false;
            client.GetComponent<networkScript>().endTurn.Value = false;
            endTurn();
        }
    }

    public void endTurn() {
        // TODO: Implement end of turn (prolly after making money)
        turnNumber += 1;
        endTurnBut.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "End Turn";
        craftingOutput.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().SetText("Craft Seed for $" + craftPrice);
        
        // Report state to analytics
        int numFarms = 0;
        foreach(GameObject farmPlot in farms) {
            if(farmPlot.GetComponent<farmInfo>().plotOwned) {
                numFarms += 1;
            }
        }

        if (NetworkManager.Singleton.IsServer) {
            ReportPlayerState("" + NetworkManager.Singleton.LocalClientId, seedsCrafted, turnNumber, money, numFarms);
        }

        // Reset turn vars
        craftPrice = BASE_CRAFT_PRICE;

        // Check/decrease farms counters. If any at 0, harvest and set seed back in inventory
        foreach (GameObject farmPlot in farms) {
            if(farmPlot.GetComponent<farmInfo>().plotOwned && farmPlot.GetComponent<farmInfo>().growTime != -1) {
                farmPlot.GetComponent<farmInfo>().growTime -= 1;
                if(farmPlot.GetComponent<farmInfo>().growTime == 0) {
                    // set growtime to -1 and reset sprite
                    farmPlot.GetComponent<farmInfo>().growTime = -1;
                    farmPlot.GetComponent<Image>().sprite = ownedSprite;
                    plantedItems -= 1;

                    // Add flowers based on harvested crops
                    Transform child = farmPlot.transform.parent.GetChild(1);
                    (int,int,int) vals = child.GetComponent<itemInfo>().getValues();

                    // Remove the seed and place into a spot
                    if (emptyInventory.Count == 0) {
                        Debug.Log("An inventory overflow error has occured");
                        return;
                    }

                    // Gets first emptyInventory slot
                    
                    GameObject item = setupItem(flowerPrefab, emptyInventory[0].transform.gameObject);
                    emptyInventory.RemoveAt(0);
                    // TODO: set the sprite with correct new sprite
                    // item.GetComponent<Image>().sprite = ;
                    item.GetComponent<flowerInfo>().petalColor = vals.Item1;
                    item.GetComponent<flowerInfo>().flowerHeight = vals.Item2;
                    item.GetComponent<flowerInfo>().growSpeed = vals.Item3;
                    item.GetComponent<Image>().sprite = flowerSprites[vals.Item1];
                    // Gets the number from the grow speed (0->1;1->3;2->5)
                    item.GetComponent<flowerInfo>().numRemaining = (vals.Item3 == 0) ? (1) : ((vals.Item3 == 1) ? (3) : (5));
                    items.Add(item);

                    // TODO: Make a flower object and add it to the inventory w/ number of flowers left



                    // Remove the seed and place into a spot
                    if (emptyInventory.Count == 0) {
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
            (int, int) vals0 = getItemInfo(child0.GetChild(0), trait);
            (int, int) vals1 = getItemInfo(child1.GetChild(0), trait);
            (string, string) strs0 = getReadableItemFormat(vals0, trait);
            (string, string) strs1 = getReadableItemFormat(vals1, trait);
            // Side vals
            square.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = strs1.Item1;
            square.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = strs1.Item2;
            square.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = strs0.Item1;
            square.transform.GetChild(6).GetChild(0).GetComponent<TextMeshProUGUI>().text = strs0.Item2;

            // Center table vals
            square.transform.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>().text = getItemsTable(vals0.Item1, vals1.Item1, trait);
            square.transform.GetChild(5).GetChild(0).GetComponent<TextMeshProUGUI>().text = getItemsTable(vals0.Item1, vals1.Item2, trait);
            square.transform.GetChild(7).GetChild(0).GetComponent<TextMeshProUGUI>().text = getItemsTable(vals0.Item2, vals1.Item1, trait);
            square.transform.GetChild(8).GetChild(0).GetComponent<TextMeshProUGUI>().text = getItemsTable(vals0.Item2, vals1.Item2, trait);

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
            if(child.name == "hoverCursor") {
                continue;
            }
            children.Add(child);
        }

        atMainMenu = false;

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

        contracts = Instantiate(layoutPrefab); // farm
        contracts.name = "contracts";
        contracts.transform.SetParent(canvas.transform);

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
        // Contracts
        layoutSetup(contracts, Mathf.FloorToInt(width * .5f), 0, Mathf.FloorToInt(height * .1f), Mathf.FloorToInt(height * .1f));

        // Adds additional constraints
        // inventory cell size -> x=160, y=154
        inventory.GetComponent<GridLayoutGroup>().cellSize = new Vector2(160, 154.5f);
        moneyBlock.GetComponent<GridLayoutGroup>().cellSize = new Vector2(width/2, Mathf.FloorToInt(height * .1f));
        craftingInput.GetComponent<GridLayoutGroup>().cellSize = new Vector2(290, 162);
        square.GetComponent<GridLayoutGroup>().cellSize = new Vector2(192, 180);
        square.GetComponent<GridLayoutGroup>().constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        square.GetComponent<GridLayoutGroup>().constraintCount = 3;
        squareTrait.GetComponent<GridLayoutGroup>().cellSize = new Vector2(383, 180);
        craftingOutput.GetComponent<GridLayoutGroup>().cellSize = new Vector2(290, 162);
        swapView.GetComponent<GridLayoutGroup>().cellSize = new Vector2(width/8, Mathf.FloorToInt(height * .1f));
        farm.GetComponent<GridLayoutGroup>().cellSize = new Vector2(320, 173);
        contracts.GetComponent<GridLayoutGroup>().cellSize = new Vector2(Mathf.FloorToInt(width * .5f), 173);

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
        Debug.Log("Finished Farm View Setup");
        contractSetup();
        Debug.Log("Finished Contracts View Setup");

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
            if (i < 3) {
                GameObject item = setupItem(itemPrefab, button);
                item.GetComponent<Image>().sprite = itemSprites[i % (itemSprites.Count)];
                item.GetComponent<itemInfo>().generation = 0;


                item.GetComponent<itemInfo>().petalColor = (initSeeds[i % 3].Item1, initSeeds[i % 3].Item2);
                item.GetComponent<itemInfo>().flowerHeight = (initSeeds[i % 3].Item1, initSeeds[i % 3].Item2);
                item.GetComponent<itemInfo>().growQuality = (initSeeds[i % 3].Item1, initSeeds[i % 3].Item2);
                
                items.Add(item);
            } else {
                emptyInventory.Add(button);
            }
        }
    }
    
    private void moneySetup() {
        GameObject button = setupButton(inventoryButtonPrefab, moneyBlock); // sets the parent to the inventory and the localScale to 1 (so it's not huge when it's made)
        button.name = "money";
        money = INIT_MONEY;
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
        GameObject squareButText0 = setupText(textPrefab, squareButton0, "");
        GameObject squareButton1 = setupButton(inventoryButtonPrefab, square);
        GameObject squareButText1 = setupText(textPrefab, squareButton1, "");
        GameObject squareButton2 = setupButton(inventoryButtonPrefab, square); // Sets up second row buttons
        GameObject squareButText2 = setupText(textPrefab, squareButton2, "");
        GameObject square0 = setupButton(inventoryButtonPrefab, square);
        GameObject squareText0 = setupText(textPrefab, square0, "");
        squareText0.GetComponent<RectTransform>().sizeDelta = new Vector2(squareText0.GetComponent<RectTransform>().sizeDelta.x-17, squareText0.GetComponent<RectTransform>().sizeDelta.y);
        GameObject square1 = setupButton(inventoryButtonPrefab, square);
        GameObject squareText1 = setupText(textPrefab, square1, "");
        squareText1.GetComponent<RectTransform>().sizeDelta = squareText0.GetComponent<RectTransform>().sizeDelta;
        GameObject squareButton3 = setupButton(inventoryButtonPrefab, square); // Sets up thrid row buttons
        GameObject squareButText3 = setupText(textPrefab, squareButton3, "");
        GameObject square2 = setupButton(inventoryButtonPrefab, square);
        GameObject squareText2 = setupText(textPrefab, square2, "");
        squareText2.GetComponent<RectTransform>().sizeDelta = squareText0.GetComponent<RectTransform>().sizeDelta;
        GameObject square3 = setupButton(inventoryButtonPrefab, square);
        GameObject squareText3 = setupText(textPrefab, square3, "");
        squareText3.GetComponent<RectTransform>().sizeDelta = squareText0.GetComponent<RectTransform>().sizeDelta;

        // Sets up the buttons to swap traits
        GameObject traitButton0 = setupButton(inventoryButtonPrefab, squareTrait);
        GameObject traitButText0 = setupText(textPrefab, traitButton0, "Color");
        traitButton0.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().setTrait(0); });
        traitButton0.GetComponent<Button>().transition = Selectable.Transition.ColorTint;
        traitButton0.GetComponent<Button>().colors = buttonColors;
        GameObject traitButton1 = setupButton(inventoryButtonPrefab, squareTrait);
        GameObject traitButText1 = setupText(textPrefab, traitButton1, "Height");
        traitButton1.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().setTrait(1); });
        traitButton1.GetComponent<Button>().transition = Selectable.Transition.ColorTint;
        traitButton1.GetComponent<Button>().colors = buttonColors;
        GameObject traitButton2 = setupButton(inventoryButtonPrefab, squareTrait);
        GameObject traitButText2 = setupText(textPrefab, traitButton2, "Grow Type");
        traitButton2.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().setTrait(2); });
        traitButton2.GetComponent<Button>().transition = Selectable.Transition.ColorTint;
        traitButton2.GetComponent<Button>().colors = buttonColors;

        // Sets up craft button and square where crafted items are placed
        GameObject doCraftButton = setupButton(inventoryButtonPrefab, craftingOutput); // sets the parent to the inventory and the localScale to 1 (so it's not huge when it's made)
        doCraftButton.name = "doCraftButton";
        GameObject doCraftText1 = setupText(textPrefab, doCraftButton, "Craft Seed for $0");
        doCraftText1.GetComponent<RectTransform>().sizeDelta = new Vector2(doCraftText1.GetComponent<RectTransform>().sizeDelta.x - 17, doCraftText1.GetComponent<RectTransform>().sizeDelta.y);
        doCraftButton.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().craftItem(doCraftButton); });
        doCraftButton.GetComponent<Button>().transition = Selectable.Transition.ColorTint;
        doCraftButton.GetComponent<Button>().colors = buttonColors;

        GameObject craftButton2 = setupButton(inventoryButtonPrefab, craftingOutput); // sets the parent to the inventory and the localScale to 1 (so it's not huge when it's made)
        craftButton2.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().inventoryButtonClick(craftButton2); });
        craftButton2.name = "craftButton2";
    }

    private void swapViewSetup() {
        //Craft button
        GameObject craftingView = setupButton(inventoryButtonPrefab, swapView); // sets the parent to the inventory and the localScale to 1 (so it's not huge when it's made)
        craftingView.name = "craftingView";
        GameObject craftingViewText = setupText(textPrefab, craftingView, "Crafting View");
        craftingView.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().switchCraftView(); });
        craftingView.GetComponent<Button>().transition = Selectable.Transition.ColorTint;
        craftingView.GetComponent<Button>().colors = buttonColors;
        // Farm button
        GameObject farmInventoryButton = setupButton(inventoryButtonPrefab, swapView); // sets the parent to the inventory and the localScale to 1 (so it's not huge when it's made)
        farmInventoryButton.name = "farmViewButton";
        GameObject farmInventoryText = setupText(textPrefab, farmInventoryButton, "Farm View");
        farmInventoryButton.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().switchFarmView(); });
        farmInventoryButton.GetComponent<Button>().transition = Selectable.Transition.ColorTint;
        farmInventoryButton.GetComponent<Button>().colors = buttonColors;
        // Contracts button
        GameObject contractsButton = setupButton(inventoryButtonPrefab, swapView);
        contractsButton.name = "contractsButton";
        GameObject contractInventoryText = setupText(textPrefab, contractsButton, "Contract View");
        contractsButton.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().switchContractView(); });
        contractsButton.GetComponent<Button>().transition = Selectable.Transition.ColorTint;
        contractsButton.GetComponent<Button>().colors = buttonColors;
        // End turn button
        GameObject endTurnButton = setupButton(inventoryButtonPrefab, swapView); // sets the parent to the inventory and the localScale to 1 (so it's not huge when it's made)
        endTurnButton.name = "endTurnButton";
        GameObject endTurnText = setupText(textPrefab, endTurnButton, "End Turn");
        endTurnButton.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().ReadyEndTurn(endTurnButton); });
        endTurnBut = endTurnButton;
        endTurnBut.GetComponent<Button>().transition = Selectable.Transition.ColorTint;
        endTurnBut.GetComponent<Button>().colors = buttonColors;

    }

    // sets up the farm plots (yours and opponents) and hides it
    private void farmSetup() {
        for (int i = 0; i < FARM_SIZE; i++) {
            // Maybe make farms their own button
            GameObject button = setupButton(inventoryButtonPrefab, farm); // sets the parent to the inventory and the localScale to 1 (so it's not huge when it's made)
            button.GetComponent<Image>().sprite = farmBackground;
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

    private void contractSetup() {
        // Make initial buttons (sell seed, sell flower, 3 empty)
        GameObject sellSeedButton = setupButton(inventoryButtonPrefab, contracts);
        sellSeedButton.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().sellItem(sellSeedButton); });
        GameObject sellSeedContract = setupItem(contractPrefab, sellSeedButton);
        sellSeedContract.GetComponent<contractInfo>().buySeed = true;
        sellSeedContract.GetComponent<contractInfo>().price = 25;
        GameObject sellSeedText = setupText(textPrefab, sellSeedButton, "Sell seeds for $25");

        GameObject sellFlowerButton = setupButton(inventoryButtonPrefab, contracts);
        sellFlowerButton.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().sellItem(sellFlowerButton); });
        GameObject sellFlowerContract = setupItem(contractPrefab, sellFlowerButton);
        sellFlowerContract.GetComponent<contractInfo>().buySeed = false;
        sellFlowerContract.GetComponent<contractInfo>().remaining = -1;
        sellFlowerContract.GetComponent<contractInfo>().price = 50;
        sellFlowerContract.GetComponent<contractInfo>().petalColor = -1;
        sellFlowerContract.GetComponent<contractInfo>().flowerHeight = -1;
        sellFlowerContract.GetComponent<contractInfo>().growSpeed = -1;
        GameObject sellFlowerText = setupText(textPrefab, sellFlowerButton, "Sell any flower for $50");

        if (NetworkManager.Singleton.IsHost) {
            for (int i = 0; i < 3; i++) {
                GameObject sellButton = setupButton(inventoryButtonPrefab, contracts);
                sellButton.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().sellItem(sellButton); });

                GameObject sellContract = setupItem(contractPrefab, sellButton);
                sellContract.GetComponent<contractInfo>().buySeed = false;

                GameObject sellText = setupText(textPrefab, sellButton, "placeholder text");
                sellText.GetComponent<RectTransform>().sizeDelta = new Vector2(sellText.GetComponent<RectTransform>().sizeDelta.x - 100, sellText.GetComponent<RectTransform>().sizeDelta.y);
                sellContract.GetComponent<contractInfo>().number = i + 1;
                sellContract.GetComponent<contractInfo>().petalColor = Mathf.FloorToInt(Random.Range(0, 3));
                sellContract.GetComponent<contractInfo>().flowerHeight = -1;
                sellContract.GetComponent<contractInfo>().growSpeed = -1;
                sellContract.GetComponent<contractInfo>().price = Mathf.FloorToInt(Random.Range(75, 125));
                sellContract.GetComponent<contractInfo>().remaining = Mathf.FloorToInt(Random.Range(1, 6));

                if (i >= 1) {
                    sellContract.GetComponent<contractInfo>().flowerHeight = Mathf.FloorToInt(Random.Range(0, 2));
                    sellContract.GetComponent<contractInfo>().price = Mathf.FloorToInt(Random.Range(150, 225));
                }
                if (i >= 2) {
                    sellContract.GetComponent<contractInfo>().growSpeed = Mathf.FloorToInt(Random.Range(0, 3));
                    int speed = sellContract.GetComponent<contractInfo>().growSpeed;
                    if (speed == 0) { // fast
                        sellContract.GetComponent<contractInfo>().price = Mathf.FloorToInt(Random.Range(800, 1000));
                        sellContract.GetComponent<contractInfo>().remaining = Mathf.FloorToInt(Random.Range(1, 2));
                    } else if (speed == 1) {
                        sellContract.GetComponent<contractInfo>().price = Mathf.FloorToInt(Random.Range(250, 350));
                        sellContract.GetComponent<contractInfo>().remaining = Mathf.FloorToInt(Random.Range(3, 7));
                    } else { // high quantity
                        sellContract.GetComponent<contractInfo>().price = Mathf.FloorToInt(Random.Range(225, 275));
                        sellContract.GetComponent<contractInfo>().remaining = Mathf.FloorToInt(Random.Range(5, 10));
                    }
                }
                sellText.GetComponent<TextMeshProUGUI>().text = setContractText(sellContract.GetComponent<contractInfo>());

                // Sets contract info
                var playerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                playerObj.GetComponent<networkScript>().SetContractInfo(i + 1, sellContract.GetComponent<contractInfo>());
            }
        } else {
            networkScript hostScript = GameObject.Find("networkPlayerHost").GetComponent<networkScript>();
            int valCheck = hostScript.contract3Remaining.Value;
            float timeoutTime = 5;
            while (valCheck == 0) {
                timeoutTime -= Time.fixedTime;
                Debug.Log("waiting for host setup to complete");
                valCheck = hostScript.contract3Remaining.Value;
                if(timeoutTime < 0) {
                    return;
                }
            }

            // Once host is done, then fill in info
            for (int i = 0; i < 3; i++) {
                GameObject sellButton = setupButton(inventoryButtonPrefab, contracts);
                sellButton.GetComponent<Button>().onClick.AddListener(delegate { gameObject.GetComponent<GameManager>().sellItem(sellButton); });
                GameObject sellContract = setupItem(contractPrefab, sellButton);
                GameObject sellText = setupText(textPrefab, sellButton, "placeholder text");
                sellContract.GetComponent<contractInfo>().buySeed = false;
                sellContract.GetComponent<contractInfo>().number = i+1;

                if (i == 0) {
                    sellContract.GetComponent<contractInfo>().petalColor = hostScript.contract1PetalColor.Value;
                    sellContract.GetComponent<contractInfo>().flowerHeight = hostScript.contract1FlowerHeight.Value;
                    sellContract.GetComponent<contractInfo>().growSpeed = hostScript.contract1GrowSpeed.Value;
                    sellContract.GetComponent<contractInfo>().price = hostScript.contract1Price.Value;
                    sellContract.GetComponent<contractInfo>().remaining = hostScript.contract1Remaining.Value;
                } else if (i == 1) {
                    sellContract.GetComponent<contractInfo>().petalColor = hostScript.contract2PetalColor.Value;
                    sellContract.GetComponent<contractInfo>().flowerHeight = hostScript.contract2FlowerHeight.Value;
                    sellContract.GetComponent<contractInfo>().growSpeed = hostScript.contract2GrowSpeed.Value;
                    sellContract.GetComponent<contractInfo>().price = hostScript.contract2Price.Value;
                    sellContract.GetComponent<contractInfo>().remaining = hostScript.contract2Remaining.Value;
                } else {
                    sellContract.GetComponent<contractInfo>().petalColor = hostScript.contract3PetalColor.Value;
                    sellContract.GetComponent<contractInfo>().flowerHeight = hostScript.contract3FlowerHeight.Value;
                    sellContract.GetComponent<contractInfo>().growSpeed = hostScript.contract3GrowSpeed.Value;
                    sellContract.GetComponent<contractInfo>().price = hostScript.contract3Price.Value;
                    sellContract.GetComponent<contractInfo>().remaining = hostScript.contract3Remaining.Value;
                }
                sellText.GetComponent<TextMeshProUGUI>().text = setContractText(sellContract.GetComponent<contractInfo>());

                //Sets contract info
                var playerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                playerObj.GetComponent<networkScript>().SetContractInfo(i + 1, sellContract.GetComponent<contractInfo>());
            }
        }

            contracts.SetActive(false);
    }

    private string setContractText(contractInfo obj) {
        string textInfo = "Buying ";

        if (obj.flowerHeight == 0) {
            textInfo += "tall ";
        } else if(obj.flowerHeight == 1) {
            textInfo += "short ";
        }

        if (obj.petalColor == 0) {
            textInfo += "red ";
        } else if (obj.petalColor == 1) {
            textInfo += "pink ";
        } else if(obj.petalColor == 2) {
            textInfo += "white ";
        }

        if (obj.growSpeed == 0) {
            textInfo += "fast growing ";
        } else if (obj.growSpeed == 1) {
            textInfo += "mixed yield/growth ";
        } else if (obj.growSpeed == 2) {
            textInfo += "high yielding ";
        }

        textInfo += "flowers for $" + obj.price + ".\nRemaining: " + obj.remaining;

        return textInfo;
    }

}
