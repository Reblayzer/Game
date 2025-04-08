import { WorldJoined, PlotPurchased } from "../generated/GameEngine/GameEngine";
import { Player, Plot } from "../generated/schema";
import { Bytes, BigInt } from "@graphprotocol/graph-ts";

export function handleWorldJoined(event: WorldJoined): void {
  const id = `${event.params.player.toHex()}-${event.params.worldId.toString()}`;
  const player = new Player(id);
  player.worldId = event.params.worldId;
  player.joinedAt = event.params.timestamp;
  player.plotsOwned = 0;
  player.save();
}

export function handlePlotPurchased(event: PlotPurchased): void {
  const plotId = `${event.params.worldId.toString()}-${event.params.x.toString()}-${event.params.y.toString()}`;
  const plot = new Plot(plotId);

  const playerId = `${event.params.player.toHex()}-${event.params.worldId.toString()}`;

  plot.owner = playerId;
  plot.worldId = event.params.worldId;
  plot.x = event.params.x;
  plot.y = event.params.y;
  plot.price = event.params.price;

  const player = Player.load(playerId);
  if (player) {
    player.plotsOwned += 1;
    player.save();
  }

  plot.save();
}
