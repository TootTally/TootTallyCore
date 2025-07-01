using Steamworks;
using System;
using System.Collections.Generic;
using TootTallyCore.Utils.Helpers;
using UnityEngine;

namespace TootTallyCore.Utils.Steam
{
    public class SteamAuthTicketHandler
    {
        private Callback<GetTicketForWebApiResponse_t> _callback;
        private Dictionary<HAuthTicket, TicketRequest> _activeRequests;

        public SteamAuthTicketHandler()
        {
            _callback = Callback<GetTicketForWebApiResponse_t>.Create(OnSteamAuthTicketResponse);
            _activeRequests = new Dictionary<HAuthTicket, TicketRequest>();
        }

        private void OnSteamAuthTicketResponse(GetTicketForWebApiResponse_t pCallback)
        {
            if (!_activeRequests.Remove(pCallback.m_hAuthTicket, out var req)) return;
            if (pCallback.m_eResult != EResult.k_EResultOK)
            {
                req.OnFailure(
                    new TicketRequestException($"Unable to get Steam auth ticket (code {pCallback.m_eResult})", pCallback.m_eResult));
                return;
            }

            var ticketBytes = new Span<byte>(pCallback.m_rgubTicket, 0, pCallback.m_cubTicket);
            var hex = HexConverter.ToHexString(ticketBytes);

            req.OnSuccess(hex);
        }

        /// <summary>
        /// Request a ticket and return the result as a coroutine
        /// </summary>
        public TicketRequest RequestTicket()
        {
            if (!SteamManager.Initialized)
            {
                throw new InvalidOperationException("Can't request ticket as Steam hasn't been initialized!");
            }

            var handle = SteamUser.GetAuthTicketForWebApi("TootTally");
            return _activeRequests[handle] = new TicketRequest();
        }
    }

    public class TicketRequest : CustomYieldInstruction
    {
        public string Result { get; private set; }
        public Exception Exception { get; private set; }

        internal TicketRequest()
        {
        }

        public override bool keepWaiting => Result == null && Exception == null;

        internal void OnSuccess(string ticket) => Result = ticket;
        internal void OnFailure(Exception exn) => Exception = exn;
    }

    public class TicketRequestException : Exception
    {
        public EResult Code { get; }

        internal TicketRequestException(string message, EResult code) : base(message)
        {
            Code = code;
        }
    }
}
