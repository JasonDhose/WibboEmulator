﻿using System.Data;
using Wibbo.Game.Rooms.AI.Responses;
using Wibbo.Game.Rooms.AI;

namespace Wibbo.Game.Bots
{
    public class BotManager
    {
        private readonly List<BotResponse> _responses;

        public BotManager()
        {
            _responses = new List<BotResponse>();
        }

        public void Init()
        {
            if (_responses.Count > 0)
                _responses.Clear();
        }

        public BotResponse GetResponse(BotAIType type, string message)
        {
            foreach (BotResponse response in _responses.Where(x => x.AiType == type).ToList())
            {
                if (response.KeywordMatched(message))
                {
                    return response;
                }
            }

            return null;
        }
    }
}
