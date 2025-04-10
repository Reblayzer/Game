// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "./GameEngine.sol";
import "./PlayerNFT.sol";

contract GameWorldFactory {
    address public owner;
    PlayerNFT public playerNFT;
    address[] public worlds;

    event WorldCreated(
        address indexed gameEngine,
        uint256 indexed worldId,
        string name
    );

    modifier onlyOwner() {
        require(msg.sender == owner, "Not owner");
        _;
    }

    constructor(address _playerNFT) {
        owner = msg.sender;
        playerNFT = PlayerNFT(_playerNFT);
    }

    function createWorld(
        string calldata name
    ) external onlyOwner returns (address) {
        GameEngine world = new GameEngine();
        world.setGasTreasury(msg.sender); // you may want to change this logic later

        playerNFT.setWhitelistedCaller(address(world), true); // whitelist engine

        worlds.push(address(world));

        emit WorldCreated(address(world), worlds.length, name);
        return address(world);
    }

    function getWorlds() external view returns (address[] memory) {
        return worlds;
    }

    function totalWorlds() external view returns (uint256) {
        return worlds.length;
    }
}
