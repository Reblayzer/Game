const { ethers } = require("hardhat");

async function main() {
  console.log("🚀 Starting GameEngine deployment...");

  const GameEngine = await ethers.getContractFactory("GameEngine");
  console.log("🚀 Deploying GameEngine...");
  const engine = await GameEngine.deploy();

  await engine.waitForDeployment();

  const address = await engine.getAddress();
  console.log("✅ GameEngine deployed to:", address);
}

main().catch((error) => {
  console.error("❌ Deployment failed:", error);
  process.exitCode = 1;
});
