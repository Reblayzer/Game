import { PlayerRegistered, WorldUpdated } from "../generated/PlayerNFT/PlayerNFT";
import { PlayerNFT, PlayerWorldStat } from "../generated/schema";

export function handlePlayerRegistered(event: PlayerRegistered): void {
  const id = event.params.user.toHex();

  const player = new PlayerNFT(id);
  player.tokenId = event.params.tokenId;
  player.username = event.params.username;
  player.registeredAt = event.block.timestamp;

  player.save();
}

export function handleWorldUpdated(event: WorldUpdated): void {
  const playerId = event.params.user.toHex();
  const worldId = event.params.worldId;
  const statId = `${playerId}-${worldId.toString()}`;

  const stat = new PlayerWorldStat(statId);
  stat.player = playerId;
  stat.worldId = worldId;
  stat.plotsOwned = event.params.plotsOwned.toI32();
  stat.powerLevel = event.params.powerLevel.toI32();
  stat.updatedAt = event.block.timestamp;

  stat.save();
}
