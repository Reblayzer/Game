import {
  WorldJoined,
  PlotPurchased,
  ResourcesUpdated,
  PlotPriceUpdated,
} from "../generated/GameEngine/GameEngine";
import { GamePlayer, GamePlot, PlayerResource, PlayerNFT } from "../generated/schema";
import { BigInt } from "@graphprotocol/graph-ts";

export function handleWorldJoined(event: WorldJoined): void {
  const id = `${event.params.player.toHex()}-${event.params.worldId.toString()}`;

  const player = new GamePlayer(id);
  player.worldId = event.params.worldId;
  player.joinedAt = event.params.timestamp;
  player.plotsOwned = 0;

  player.save();
}

export function handlePlotPurchased(event: PlotPurchased): void {
  const plotId = `${event.params.worldId.toString()}-${event.params.x.toString()}-${event.params.y.toString()}`;
  const plot = new GamePlot(plotId);

  const playerId = `${event.params.player.toHex()}-${event.params.worldId.toString()}`;

  plot.owner = playerId;
  plot.worldId = event.params.worldId;
  plot.x = event.params.x;
  plot.y = event.params.y;
  plot.price = event.params.price;

  const player = GamePlayer.load(playerId);
  if (player) {
    player.plotsOwned += 1;
    player.save();
  }

  plot.save();
}

export function handleResourcesUpdated(event: ResourcesUpdated): void {
  const playerId = event.params.player.toHex();
  const id = `${playerId}-${event.params.worldId.toString()}`;

  // Ensure PlayerNFT exists
  let playerNFT = PlayerNFT.load(playerId);
  if (!playerNFT) {
    playerNFT = new PlayerNFT(playerId);
    playerNFT.tokenId = BigInt.zero(); // fallback values
    playerNFT.username = "unknown";
    playerNFT.registeredAt = event.block.timestamp;
    playerNFT.save();
  }

  let entity = PlayerResource.load(id);
  if (!entity) {
    entity = new PlayerResource(id);
    entity.player = playerId;
    entity.worldId = event.params.worldId;
    entity.plotPrice = BigInt.zero(); // Default if not set yet
  }

  const resources = event.params.newResources;
  entity.r1 = resources[0];
  entity.r2 = resources[1];
  entity.r3 = resources[2];
  entity.r4 = resources[3];
  entity.r5 = resources[4];
  entity.updatedAt = event.block.timestamp;

  entity.save();
}

export function handlePlotPriceUpdated(event: PlotPriceUpdated): void {
  const playerId = event.params.player.toHex();
  const id = `${playerId}-${event.params.worldId.toString()}`;

  // Ensure PlayerNFT exists
  let playerNFT = PlayerNFT.load(playerId);
  if (!playerNFT) {
    playerNFT = new PlayerNFT(playerId);
    playerNFT.tokenId = BigInt.zero();
    playerNFT.username = "unknown";
    playerNFT.registeredAt = event.block.timestamp;
    playerNFT.save();
  }

  let entity = PlayerResource.load(id);
  if (!entity) {
    entity = new PlayerResource(id);
    entity.player = playerId;
    entity.worldId = event.params.worldId;
    entity.r1 = BigInt.zero();
    entity.r2 = BigInt.zero();
    entity.r3 = BigInt.zero();
    entity.r4 = BigInt.zero();
    entity.r5 = BigInt.zero();
  }

  entity.plotPrice = event.params.newPrice;
  entity.updatedAt = event.block.timestamp;
  entity.save();
}