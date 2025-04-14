import { PlayerRegistered } from "../generated/PlayerNFT/PlayerNFT";
import { PlayerNFT } from "../generated/schema";

export function handlePlayerRegistered(event: PlayerRegistered): void {
  const id = event.params.user.toHex();

  const player = new PlayerNFT(id);
  player.tokenId = event.params.tokenId;
  player.username = event.params.username;
  player.registeredAt = event.block.timestamp;

  player.save();
}