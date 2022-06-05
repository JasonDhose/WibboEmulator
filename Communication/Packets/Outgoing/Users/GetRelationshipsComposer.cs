using Wibbo.Game.Users;
using Wibbo.Game.Users.Relationships;

namespace Wibbo.Communication.Packets.Outgoing.Users
{
    internal class GetRelationshipsComposer : ServerPacket
    {
        public GetRelationshipsComposer(int UserId, List<Relationship> Relationships)
            : base(ServerPacketHeader.MESSENGER_RELATIONSHIPS)
        {
            this.WriteInteger(UserId);
            this.WriteInteger(Relationships.Count);
            Random rand = new Random();
            ICollection<Relationship>relations = Relationships;

            Dictionary<int, Relationship> RelationRandom = new Dictionary<int, Relationship>();

            foreach (Relationship UserRelation in relations)
            {
                RelationRandom.Add(UserRelation.UserId, UserRelation);
            }

            RelationRandom = RelationRandom.OrderBy(x => rand.Next()).ToDictionary(item => item.Key, item => item.Value);

            int Loves = RelationRandom.Count(x => x.Value.Type == 1);
            int Likes = RelationRandom.Count(x => x.Value.Type == 2);
            int Hates = RelationRandom.Count(x => x.Value.Type == 3);
            foreach (Relationship Rel in RelationRandom.Values)
            {
                User HHab = WibboEnvironment.GetUserById(Rel.UserId);
                if (HHab == null)
                {
                    base.WriteInteger(0);
                    base.WriteInteger(0);
                    base.WriteInteger(0); // Their ID
                    base.WriteString("Placeholder");
                    base.WriteString("hr-115-42.hd-190-1.ch-215-62.lg-285-91.sh-290-62");
                }
                else
                {
                    base.WriteInteger(Rel.Type);
                    base.WriteInteger(Rel.Type == 1 ? Loves : Rel.Type == 2 ? Likes : Hates);
                    base.WriteInteger(Rel.UserId); // Their ID
                    base.WriteString(HHab.Username);
                    base.WriteString(HHab.Look);
                }
            }
        }
    }
}
