// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

contract GameRegistry {
    struct Player {
        string username;
        bool registered;
    }

    mapping(address => Player) public players;
    mapping(string => bool) public takenUsernames;

    event PlayerRegistered(address indexed player, string username);

    function register(string calldata username) external {
        require(!players[msg.sender].registered, "Already registered");
        require(!takenUsernames[username], "Username already taken");

        players[msg.sender] = Player({
            username: username,
            registered: true
        });

        takenUsernames[username] = true;

        emit PlayerRegistered(msg.sender, username);
    }

    function getUsername(address player) external view returns (string memory) {
        return players[player].username;
    }

    function isRegistered(address player) external view returns (bool) {
        return players[player].registered;
    }
}
