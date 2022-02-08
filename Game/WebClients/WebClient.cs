﻿using Butterfly.Communication.Packets.Incoming;
using Butterfly.Communication.Packets.Outgoing;
using Butterfly.Communication.Packets.Outgoing.WebSocket;
using Butterfly.Communication.WebSocket;
using Butterfly.Core;
using Butterfly.Database.Daos;
using Butterfly.Database.Interfaces;
using Butterfly.Net;
using ConnectionManager;
using System;
using System.Data;

namespace Butterfly.Game.WebClients
{
    public class WebClient
    {
        private readonly ConnectionInformation _connection;
        private readonly WebPacketParser _packetParser;

        private bool _isStaff;
        private bool _showGameAlert;

        private Language _langue;

        public int UserId { get; set; }

        public int ConnectionID { get; set; }

        public bool ShowGameAlert { get => this._showGameAlert; set => this._showGameAlert = value; }
        public Language Langue { get => this._langue; set => this._langue = value; }

        public WebClient(int id, ConnectionInformation connection)
        {
            this.ConnectionID = id;

            this._isStaff = false;
            this._showGameAlert = true;
            this._connection = connection;
            this._packetParser = new WebPacketParser(this);
        }

        public void TryAuthenticate(string authTicket)
        {
            if (string.IsNullOrEmpty(authTicket))
            {
                return;
            }

            string ip = this.GetConnection().GetIp();

            using (IQueryAdapter dbClient = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                bool isBanned = BanDao.IsBannedByIP(dbClient, ButterflyEnvironment.GetUnixTimestamp(), ip);

                if (isBanned)
                {
                    return;
                }

                DataRow dUserInfo = UserWebsocketDao.GetOne(dbClient, authTicket);
                if (dUserInfo == null)
                {
                    return;
                }

                this.UserId = Convert.ToInt32(dUserInfo["user_id"]);
                this._isStaff = ButterflyEnvironment.EnumToBool((string)dUserInfo["is_staff"]);
                this._langue = LanguageManager.ParseLanguage(Convert.ToString(dUserInfo["langue"]));
                UserWebsocketDao.UpdateTicket(dbClient, this.UserId);

                this.SendPacket(new AuthOkComposer());

                this.SendSettingSound(dbClient);
            }

            ButterflyEnvironment.GetGame().GetClientWebManager().LogClonesOut(this.UserId);
            ButterflyEnvironment.GetGame().GetClientWebManager().RegisterClient(this, this.UserId);

            this.SendPacket(new UserIsStaffComposer(this._isStaff));
        }

        public void Disconnect()
        {
            this._isStaff = false;
            this.UserId = 0;

            ButterflyEnvironment.GetGame().GetClientWebManager().UnregisterClient(this.UserId);
        }

        private void SendSettingSound(IQueryAdapter dbClient)
        {
            DataRow dUserVolume = UserDao.GetOneVolume(dbClient, this.UserId);
            if (dUserVolume == null)
            {
                return;
            }

            string clientVolume = dUserVolume["volume"].ToString();

            if (clientVolume.Contains(","))
            {
                string[] Str = clientVolume.Split(',');
                if (Str.Length != 3)
                {
                    return;
                }

                int.TryParse(Str[0], out int systemSound);
                int.TryParse(Str[1], out int furniSound);
                int.TryParse(Str[2], out int traxSound);

                this.SendPacket(new SettingVolumeComposer(traxSound, furniSound, systemSound));
            }
        }

        private void SwitchParserRequest()
        {
            this._packetParser.OnNewPacket += new WebPacketParser.HandlePacket(this.Parser_onNewPacket);

            byte[] packet = (this._connection.Parser as InitialPacketParser).CurrentData;
            this._connection.Parser.Dispose();
            this._connection.Parser = this._packetParser;
            this._connection.Parser.HandlePacketData(packet);
        }

        private void Parser_onNewPacket(ClientPacket Message)
        {
            try
            {
                ButterflyEnvironment.GetGame().GetPacketManager().TryExecuteWebPacket(this, Message);
            }
            catch (Exception ex)
            {
                Logging.LogPacketException(Message.ToString(), (ex).ToString());
            }
        }

        public ConnectionInformation GetConnection()
        {
            return this._connection;
        }

        public void StartConnection()
        {
            if (this._connection == null)
            {
                return;
            } (this._connection.Parser as InitialPacketParser).SwitchParserRequest += new InitialPacketParser.NoParamDelegate(this.SwitchParserRequest);

            this._connection.StartPacketProcessing();
        }

        public void Dispose()
        {
            if (this._connection != null)
            {
                this._connection.Dispose();
            }
        }

        public void SendPacket(IServerPacket Message)
        {
            if (Message == null || this.GetConnection() == null)
            {
                return;
            }

            this.GetConnection().SendData(EncodeDecode.EncodeMessage(Message.GetBytes()));
        }
    }
}
