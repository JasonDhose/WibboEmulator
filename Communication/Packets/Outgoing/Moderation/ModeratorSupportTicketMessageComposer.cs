using Butterfly.Game.Moderation;
using Butterfly.Utility;
using System;

namespace Butterfly.Communication.Packets.Outgoing.Moderation
{
    internal class ModeratorSupportTicketMessageComposer : ServerPacket
    {
        public ModeratorSupportTicketMessageComposer(int Id, SupportTicket Ticket)
            : base(ServerPacketHeader.ModeratorSupportTicketMessageComposer)
        {
            WriteInteger(Ticket.Id); // Id
            WriteInteger(Ticket.GetStatus(Id)); // Tab ID
            WriteInteger(Ticket.Type); // Type
            WriteInteger(Ticket.Category); // Category 
            WriteInteger(Convert.ToInt32((DateTime.Now - UnixTimestamp.FromUnixTimestamp(Ticket.Timestamp)).TotalMilliseconds)); // This should fix the overflow?
            WriteInteger(Ticket.Priority); // Priority
            WriteInteger(0);//??
            WriteInteger(Ticket.Sender == null ? 0 : Ticket.Sender.Id); // Sender ID
                                                                        //base.WriteInteger(1);
            WriteString(Ticket.Sender == null ? string.Empty : Ticket.Sender.Username); // Sender Name
            WriteInteger(Ticket.Reported == null ? 0 : Ticket.Reported.Id); // Reported ID
            WriteString(Ticket.Reported == null ? string.Empty : Ticket.Reported.Username); // Reported Name
            WriteInteger(Ticket.Moderator == null ? 0 : Ticket.Moderator.Id); // Moderator ID
            WriteString(Ticket.Moderator == null ? string.Empty : Ticket.Moderator.Username); // Mod Name
            WriteString(Ticket.Issue); // Issue
            WriteInteger(Ticket.Room == null ? 0 : Ticket.Room.Id); // Room Id
            WriteInteger(0);
            {
                // push String
                // push Integer
                // push Integer
            }
        }
    }
}
