// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

contract GameEngineBackup {
    address public owner;
    bool public paused = false;
    uint256 public constant JOIN_FEE = 10 ether; // 10 tHYDRA

    struct PlayerWorld {
        bool joined;
        uint256 joinedAt;
    }

    mapping(address => mapping(uint256 => PlayerWorld)) public playerWorlds;

    modifier onlyOwner() {
        require(msg.sender == owner, "Not the owner");
        _;
    }

    modifier notPaused() {
        require(!paused, "Game is paused");
        _;
    }

    event WorldJoined(address indexed player, uint256 indexed worldId, uint256 timestamp);
    event FundsWithdrawn(address indexed to, uint256 amount);

    constructor() {
        owner = msg.sender;
    }

    function joinWorld(uint256 worldId) external payable notPaused {
        require(worldId > 0, "Invalid world");
        PlayerWorld storage pw = playerWorlds[msg.sender][worldId];

        require(!pw.joined, "Already joined");
        require(msg.value == JOIN_FEE, "Must pay 10 tHYDRA to join");

        pw.joined = true;
        pw.joinedAt = block.timestamp;

        emit WorldJoined(msg.sender, worldId, block.timestamp);
    }

    function hasJoined(address player, uint256 worldId) external view returns (bool) {
        return playerWorlds[player][worldId].joined;
    }

    function pauseGame() external onlyOwner {
        paused = true;
    }

    function resumeGame() external onlyOwner {
        paused = false;
    }

    function withdraw() external onlyOwner {
        uint256 amount = address(this).balance;
        require(amount > 0, "No funds");
        (bool success, ) = payable(owner).call{value: amount}("");
        require(success, "Withdraw failed");

        emit FundsWithdrawn(owner, amount);
    }
}
