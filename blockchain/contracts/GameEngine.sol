// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

contract GameEngine {
    address public owner;
    address public gasTreasury;
    bool public paused = false;

    uint256 public constant JOIN_FEE = 10 ether;
    uint256 public constant STARTING_RESOURCE = 10000;
    uint8 public constant GRID_SIZE = 10;
    uint256 public constant BASE_PRICE = 100;

    enum Resource {
        R1,
        R2,
        R3,
        R4,
        R5
    }

    struct PlayerWorld {
        bool joined;
        uint256 joinedAt;
        uint256 ownedPlots;
        mapping(uint8 => uint256) resources;
    }

    struct Plot {
        address owner;
        uint8 x;
        uint8 y;
    }

    mapping(address => mapping(uint256 => PlayerWorld)) internal playerWorlds;
    mapping(uint256 => mapping(uint8 => mapping(uint8 => Plot)))
        public worldPlots;

    modifier onlyOwner() {
        require(msg.sender == owner, "Not the owner");
        _;
    }

    modifier onlyGasTreasury() {
        require(msg.sender == gasTreasury, "Only gas treasury");
        _;
    }

    modifier notPaused() {
        require(!paused, "Game is paused");
        _;
    }

    // --- Events ---
    event WorldJoined(
        address indexed player,
        uint256 indexed worldId,
        uint256 timestamp
    );
    event PlotPurchased(
        address indexed player,
        uint256 indexed worldId,
        uint8 x,
        uint8 y,
        uint256 price
    );
    event ResourcesUpdated(
        address indexed player,
        uint256 indexed worldId,
        uint256[5] newResources
    );
    event PlotPriceUpdated(
        address indexed player,
        uint256 indexed worldId,
        uint256 newPrice
    );
    event StatsUpdated(
        address indexed player,
        uint256 indexed worldId,
        uint256 plotsOwned,
        uint256 powerLevel
    );

    constructor() {
        owner = msg.sender;
        gasTreasury = msg.sender;
    }

    function setGasTreasury(address _gasTreasury) external onlyOwner {
        gasTreasury = _gasTreasury;
    }

    function pauseGame() external onlyOwner {
        paused = true;
    }

    function resumeGame() external onlyOwner {
        paused = false;
    }

    function joinWorld(uint256 worldId) external payable notPaused {
        require(worldId > 0, "Invalid world");
        PlayerWorld storage pw = playerWorlds[msg.sender][worldId];
        require(!pw.joined, "Already joined");
        require(msg.value == JOIN_FEE, "Must pay 10 tHYDRA to join");

        pw.joined = true;
        pw.joinedAt = block.timestamp;

        for (uint8 i = 0; i < 5; i++) {
            pw.resources[i] = STARTING_RESOURCE;
        }

        emit WorldJoined(msg.sender, worldId, block.timestamp);
        emit ResourcesUpdated(msg.sender, worldId, getResourcesArray(pw));
        emit PlotPriceUpdated(
            msg.sender,
            worldId,
            getPlotPrice(worldId, msg.sender)
        );
        emit StatsUpdated(
            msg.sender,
            worldId,
            pw.ownedPlots,
            calculatePowerLevel(pw)
        );
    }

    function hasJoined(
        address player,
        uint256 worldId
    ) external view returns (bool) {
        return playerWorlds[player][worldId].joined;
    }

    function getPlayerResources(
        uint256 worldId,
        address player
    ) external view returns (uint256[5] memory) {
        PlayerWorld storage pw = playerWorlds[player][worldId];
        return getResourcesArray(pw);
    }

    function getPlayerState(
        uint256 worldId
    )
        external
        view
        returns (bool joined, uint256 plots, uint256[5] memory res)
    {
        PlayerWorld storage pw = playerWorlds[msg.sender][worldId];
        return (pw.joined, pw.ownedPlots, getResourcesArray(pw));
    }

    function getPlotOwner(
        uint256 worldId,
        uint8 x,
        uint8 y
    ) external view returns (address) {
        return worldPlots[worldId][x][y].owner;
    }

    function getAllPlotOwners(
        uint256 worldId
    ) external view returns (address[100] memory) {
        address[100] memory owners;
        uint index = 0;
        for (uint8 x = 0; x < GRID_SIZE; x++) {
            for (uint8 y = 0; y < GRID_SIZE; y++) {
                owners[index++] = worldPlots[worldId][x][y].owner;
            }
        }
        return owners;
    }

    function getPlotPrice(
        uint256 worldId,
        address player
    ) public view returns (uint256) {
        PlayerWorld storage pw = playerWorlds[player][worldId];
        return (BASE_PRICE * (15 ** pw.ownedPlots)) / (10 ** pw.ownedPlots);
    }

    function buyPlot(uint256 worldId, uint8 x, uint8 y) external notPaused {
        require(x < GRID_SIZE && y < GRID_SIZE, "Invalid coordinates");

        PlayerWorld storage pw = playerWorlds[msg.sender][worldId];
        require(pw.joined, "Not in world");
        require(worldPlots[worldId][x][y].owner == address(0), "Already owned");

        uint256 price = getPlotPrice(worldId, msg.sender);

        for (uint8 i = 0; i < 5; i++) {
            require(pw.resources[i] >= price, "Not enough resources");
            pw.resources[i] -= price;
        }

        worldPlots[worldId][x][y] = Plot({owner: msg.sender, x: x, y: y});
        pw.ownedPlots++;

        emit PlotPurchased(msg.sender, worldId, x, y, price);
        emit ResourcesUpdated(msg.sender, worldId, getResourcesArray(pw));
        emit PlotPriceUpdated(
            msg.sender,
            worldId,
            getPlotPrice(worldId, msg.sender)
        );
        emit StatsUpdated(
            msg.sender,
            worldId,
            pw.ownedPlots,
            calculatePowerLevel(pw)
        );
    }

    function withdraw() external onlyOwner {
        uint256 amount = address(this).balance;
        require(amount > 0, "No funds");
        (bool success, ) = payable(owner).call{value: amount}("");
        require(success, "Withdraw failed");
    }

    // --- Internal Utilities ---

    function getResourcesArray(
        PlayerWorld storage pw
    ) internal view returns (uint256[5] memory) {
        return [
            pw.resources[0],
            pw.resources[1],
            pw.resources[2],
            pw.resources[3],
            pw.resources[4]
        ];
    }

    function calculatePowerLevel(
        PlayerWorld storage pw
    ) internal view returns (uint256) {
        uint256 sum = 0;
        for (uint8 i = 0; i < 5; i++) {
            sum += pw.resources[i];
        }
        return sum / 5;
    }
}
