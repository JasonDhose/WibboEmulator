﻿using Butterfly.Communication.Packets.Outgoing;
using Butterfly.Database.Interfaces;
using Butterfly.Game.Clients;
using Butterfly.Game.Items;
using Butterfly.Game.Rooms.Map.Movement;
using Butterfly.Game.Rooms.Wired.WiredHandlers.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace Butterfly.Game.Rooms.Wired.WiredHandlers.Effects
{
    public class MoveToDir : IWiredEffect, IWired
    {
        private Room room;
        private WiredHandler handler;
        private readonly int itemID;
        private List<Item> items;
        private bool isDisposed;
        private MovementDirection movetodirMovement;
        private MovementDirection startDirection;
        private WhenMovementBlock whenMoveIsBlocked;

        public MoveToDir(List<Item> items, Room room, WiredHandler handler, int itemID, int StartDirection, int WhenMoveIsBlocked)
        {
            this.items = items;
            this.room = room;
            this.handler = handler;
            this.itemID = itemID;
            this.isDisposed = false;

            this.startDirection = (MovementDirection)StartDirection;
            this.movetodirMovement = this.startDirection;
            this.whenMoveIsBlocked = (WhenMovementBlock)WhenMoveIsBlocked;
        }

        public void Handle(RoomUser user, Item TriggerItem)
        {
            this.HandleItems();
        }

        public void Dispose()
        {
            this.isDisposed = true;
            this.room = null;
            this.handler = null;
            if (this.items != null)
            {
                this.items.Clear();
            }

            this.items = null;
        }

        private void HandleItems()
        {
            foreach (Item roomItem in this.items.ToArray())
            {
                this.HandleMovement(roomItem);
            }
        }

        private void HandleMovement(Item item)
        {
            if (this.room.GetRoomItemHandler().GetItem(item.Id) == null)
            {
                return;
            }

            Point newPoint = MovementManagement.HandleMovementDir(item.GetX, item.GetY, this.movetodirMovement);

            RoomUser roomUser = this.room.GetRoomUserManager().GetUserForSquare(newPoint.X, newPoint.Y);
            if (roomUser != null) // colisión
            {
                this.handler.TriggerCollision(roomUser, item);
                return;
            }

            //Point newPoint = base.HandleMovement(item.Coordinate, this.startDirection);

            int OldX = item.GetX;
            int OldY = item.GetY;
            double OldZ = item.GetZ;
            if (this.room.GetRoomItemHandler().SetFloorItem(null, item, newPoint.X, newPoint.Y, item.Rotation, false, false, false))
            {
                ServerPacket Message = new ServerPacket(ServerPacketHeader.ROOM_ROLLING);
                Message.WriteInteger(OldX);
                Message.WriteInteger(OldY);
                Message.WriteInteger(newPoint.X);
                Message.WriteInteger(newPoint.Y);
                Message.WriteInteger(1);
                Message.WriteInteger(item.Id);
                Message.WriteString(OldZ.ToString().Replace(',', '.'));
                Message.WriteString(item.GetZ.ToString().Replace(',', '.'));
                Message.WriteInteger(0);
                this.room.SendPacket(Message);
                return;
            }

            switch (this.whenMoveIsBlocked)
            {
                #region None
                case WhenMovementBlock.none:
                    {
                        //this.movetodirMovement = MovementDirection.none;
                        break;
                    }
                #endregion
                #region Right45
                case WhenMovementBlock.right45:
                    {
                        if (this.movetodirMovement == MovementDirection.right)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.left)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.up)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.down)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.upleft)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.upright)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            return;
                        }
                        else if (this.movetodirMovement == MovementDirection.downright)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.downleft)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                        }

                        break;
                    }
                #endregion
                #region Right90
                case WhenMovementBlock.right90:
                    {
                        if (this.movetodirMovement == MovementDirection.right)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.left)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.up)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.down)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.upleft)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.upright)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            return;
                        }
                        else if (this.movetodirMovement == MovementDirection.downright)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.downleft)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                        }

                        break;
                    }
                #endregion
                #region Left45
                case WhenMovementBlock.left45:
                    {
                        if (this.movetodirMovement == MovementDirection.right)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.left)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.up)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.down)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.upleft)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.upright)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.downright)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.downleft)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                        }

                        break;
                    }
                #endregion
                #region Left90
                case WhenMovementBlock.left90:
                    {
                        if (this.movetodirMovement == MovementDirection.right)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.left)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.up)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.down)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY)) // derecha
                            {
                                this.movetodirMovement = MovementDirection.right;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY - 1)) // arriba
                            {
                                this.movetodirMovement = MovementDirection.up;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY)) // izq
                            {
                                this.movetodirMovement = MovementDirection.left;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX, item.GetY + 1)) // abajo
                            {
                                this.movetodirMovement = MovementDirection.down;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.upleft)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.upright)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.downright)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                        }
                        else if (this.movetodirMovement == MovementDirection.downleft)
                        {
                            if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY + 1)) // abajo derecha
                            {
                                this.movetodirMovement = MovementDirection.downright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX + 1, item.GetY - 1)) // arriba derecha
                            {
                                this.movetodirMovement = MovementDirection.upright;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY - 1)) // arriba izq
                            {
                                this.movetodirMovement = MovementDirection.upleft;
                                break;
                            }
                            else if (this.room.GetGameMap().CanStackItem(item.GetX - 1, item.GetY + 1)) // abajo izq
                            {
                                this.movetodirMovement = MovementDirection.downleft;
                                break;
                            }
                        }

                        break;
                    }
                #endregion
                #region Turn Back
                case WhenMovementBlock.turnback:
                    {
                        if (this.movetodirMovement == MovementDirection.right)
                        {
                            this.movetodirMovement = MovementDirection.left;
                        }
                        else if (this.movetodirMovement == MovementDirection.left)
                        {
                            this.movetodirMovement = MovementDirection.right;
                        }
                        else if (this.movetodirMovement == MovementDirection.up)
                        {
                            this.movetodirMovement = MovementDirection.down;
                        }
                        else if (this.movetodirMovement == MovementDirection.down)
                        {
                            this.movetodirMovement = MovementDirection.up;
                        }
                        else if (this.movetodirMovement == MovementDirection.upright)
                        {
                            this.movetodirMovement = MovementDirection.downleft;
                        }
                        else if (this.movetodirMovement == MovementDirection.downleft)
                        {
                            this.movetodirMovement = MovementDirection.upright;
                        }
                        else if (this.movetodirMovement == MovementDirection.upleft)
                        {
                            this.movetodirMovement = MovementDirection.downright;
                        }
                        else if (this.movetodirMovement == MovementDirection.downright)
                        {
                            this.movetodirMovement = MovementDirection.upleft;
                        }
                        break;
                    }
                #endregion
                #region Random
                case WhenMovementBlock.turnrandom:
                    {
                        this.movetodirMovement = (MovementDirection)new Random().Next(1, 7);
                        break;
                    }
                    #endregion
            }

            newPoint = MovementManagement.HandleMovementDir(item.GetX, item.GetY, this.movetodirMovement);

            if (newPoint != item.Coordinate)
            {
                OldX = item.GetX;
                OldY = item.GetY;
                OldZ = item.GetZ;

                if (this.room.GetRoomItemHandler().SetFloorItem(null, item, newPoint.X, newPoint.Y, item.Rotation, false, false, false))
                {
                    ServerPacket Message = new ServerPacket(ServerPacketHeader.ROOM_ROLLING);
                    Message.WriteInteger(OldX);
                    Message.WriteInteger(OldY);
                    Message.WriteInteger(newPoint.X);
                    Message.WriteInteger(newPoint.Y);
                    Message.WriteInteger(1);
                    Message.WriteInteger(item.Id);
                    Message.WriteString(OldZ.ToString().Replace(',', '.'));
                    Message.WriteString(item.GetZ.ToString().Replace(',', '.'));
                    Message.WriteInteger(0);
                    this.room.SendPacket(Message);
                }
            }
            return;
        }

        public void SaveToDatabase(IQueryAdapter dbClient)
        {
            WiredUtillity.SaveTriggerItem(dbClient, this.itemID, Convert.ToInt32(this.startDirection).ToString(), Convert.ToInt32(this.whenMoveIsBlocked).ToString(), false, this.items);
        }

        public void LoadFromDatabase(DataRow row, Room insideRoom)
        {
            string triggerItem = row["triggers_item"].ToString();

            if (int.TryParse(row["trigger_data_2"].ToString(), out int startDirection))
                this.startDirection = (MovementDirection)startDirection;

            this.movetodirMovement = this.startDirection;

            if (int.TryParse(row["trigger_data"].ToString(), out int whenMoveIsBlocked))
                this.whenMoveIsBlocked = (WhenMovementBlock)whenMoveIsBlocked;

            if (triggerItem == "")
            {
                return;
            }

            foreach (string itemid in triggerItem.Split(';'))
            {
                Item roomItem = insideRoom.GetRoomItemHandler().GetItem(Convert.ToInt32(itemid));
                if (roomItem != null && !this.items.Contains(roomItem) && roomItem.Id != this.itemID)
                {
                    this.items.Add(roomItem);
                }
            }
        }

        public void OnTrigger(Client Session, int SpriteId)
        {
            ServerPacket Message = new ServerPacket(ServerPacketHeader.WIRED_ACTION);
            Message.WriteBoolean(false);
            Message.WriteInteger(10);
            Message.WriteInteger(this.items.Count);
            foreach (Item roomItem in this.items)
            {
                Message.WriteInteger(roomItem.Id);
            }

            Message.WriteInteger(SpriteId);
            Message.WriteInteger(this.itemID);
            Message.WriteString("");
            Message.WriteInteger(2);

            Message.WriteInteger((this.startDirection == MovementDirection.none) ? 0 : Convert.ToInt32(this.startDirection)); //Stardirection
            Message.WriteInteger(Convert.ToInt32(this.whenMoveIsBlocked)); //WhenmoveIsblocked

            Message.WriteInteger(0);
            Message.WriteInteger(13);
            Message.WriteInteger(0);
            Message.WriteInteger(0);
            Session.SendPacket(Message);
        }

        public bool Disposed()
        {
            return this.isDisposed;
        }
    }
}
