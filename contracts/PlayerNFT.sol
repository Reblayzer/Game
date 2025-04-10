// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC721/extensions/ERC721URIStorage.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

contract PlayerNFT is ERC721URIStorage, Ownable {
    struct WorldStats {
        uint256 joinedAt;
        uint256 plotsOwned;
        uint256 powerLevel;
    }

    struct Player {
        string username;
        mapping(uint256 => WorldStats) worlds;
    }

    mapping(address => bool) public hasMinted;
    mapping(uint256 => Player) private playerData;
    uint256 public nextTokenId = 1;

    mapping(address => bool) public whitelistedCallers;

    event PlayerRegistered(
        address indexed user,
        string username,
        uint256 tokenId
    );
    event WorldUpdated(
        address indexed user,
        uint256 worldId,
        uint256 plotsOwned,
        uint256 powerLevel
    );
    event CallerWhitelisted(address caller, bool status);

    constructor() ERC721("PlayerNFT", "PLNFT") Ownable() {}

    modifier onlyWhitelisted() {
        require(whitelistedCallers[msg.sender], "Caller not whitelisted");
        _;
    }

    function setWhitelistedCaller(
        address caller,
        bool approved
    ) external onlyOwner {
        whitelistedCallers[caller] = approved;
        emit CallerWhitelisted(caller, approved);
    }

    function registerPlayer(string memory username) external {
        require(!hasMinted[msg.sender], "Already registered");

        uint256 tokenId = nextTokenId++;
        _mint(msg.sender, tokenId);
        playerData[tokenId].username = username;
        hasMinted[msg.sender] = true;

        emit PlayerRegistered(msg.sender, username, tokenId);
    }

    function updateWorldStats(
        address user,
        uint256 worldId,
        uint256 plotsOwned,
        uint256 powerLevel
    ) external onlyWhitelisted {
        require(hasMinted[user], "Not registered");
        uint256 tokenId = tokenOfOwner(user);
        playerData[tokenId].worlds[worldId] = WorldStats({
            joinedAt: block.timestamp,
            plotsOwned: plotsOwned,
            powerLevel: powerLevel
        });

        emit WorldUpdated(user, worldId, plotsOwned, powerLevel);
    }

    function getWorldStats(
        address user,
        uint256 worldId
    ) external view returns (WorldStats memory) {
        require(hasMinted[user], "Not registered");
        uint256 tokenId = tokenOfOwner(user);
        return playerData[tokenId].worlds[worldId];
    }

    function getUsername(address user) external view returns (string memory) {
        require(hasMinted[user], "Not registered");
        uint256 tokenId = tokenOfOwner(user);
        return playerData[tokenId].username;
    }

    function tokenOfOwner(address user) internal view returns (uint256) {
        for (uint256 i = 1; i < nextTokenId; i++) {
            if (ownerOf(i) == user) {
                return i;
            }
        }
        revert("Token not found");
    }

    // ❌ Disable all transfer functions
    function transferFrom(address, address, uint256) public pure override {
        revert("Transfers disabled");
    }

    function safeTransferFrom(address, address, uint256) public pure override {
        revert("Transfers disabled");
    }

    function safeTransferFrom(
        address,
        address,
        uint256,
        bytes memory
    ) public pure override {
        revert("Transfers disabled");
    }

    // ✅ Required overrides for ERC721URIStorage only
    function _burn(uint256 tokenId) internal override(ERC721URIStorage) {
        super._burn(tokenId);
    }

    function tokenURI(
        uint256 tokenId
    ) public view override(ERC721URIStorage) returns (string memory) {
        return super.tokenURI(tokenId);
    }

    function supportsInterface(
        bytes4 interfaceId
    ) public view override(ERC721) returns (bool) {
        return super.supportsInterface(interfaceId);
    }
}
