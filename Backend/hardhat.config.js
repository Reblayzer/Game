require("@nomiclabs/hardhat-ethers");
require("dotenv").config();

module.exports = {
  solidity: "0.8.20",
  networks: {
    hydraTestnet: {
      url: "https://rpc-testnet.hydrachain.org",
      chainId: 8844,
      accounts: [process.env.PRIVATE_KEY]
    }
  }
};
// 