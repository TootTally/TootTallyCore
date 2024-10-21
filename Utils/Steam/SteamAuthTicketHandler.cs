using Steamworks;
using System;
using TootTallyCore.Utils.Helpers;

namespace TootTallyCore.Utils.Steam
{
    public static class SteamAuthTicketHandler
    {
        public static string SteamTicket { get; private set; }
        private static Callback<GetTicketForWebApiResponse_t> m_GetAuthSessionTicketResponse;
        private static HAuthTicket hAuthTicket;

        private static void OnSteamAuthTicketResponse(GetTicketForWebApiResponse_t pCallback)
        {
            if (pCallback.m_eResult != EResult.k_EResultOK)
            {
                Plugin.LogError($"Unable to get Steam auth ticket (Steam API Result Code {pCallback.m_eResult})");
            }
            if (pCallback.m_hAuthTicket == hAuthTicket)
            {
                var ticketBytes = new Span<byte>(pCallback.m_rgubTicket, 0, pCallback.m_cubTicket);
                var hex = HexConverter.ToHexString(ticketBytes);

                SteamTicket = hex;

                Plugin.LogInfo($"Got Steam auth ticket: '{hex}'");
            }
        }

        public static void GetSteamAuthTicket()
        {
            if (SteamManager.Initialized)
            {
                m_GetAuthSessionTicketResponse = Callback<GetTicketForWebApiResponse_t>.Create(OnSteamAuthTicketResponse);
                hAuthTicket = SteamUser.GetAuthTicketForWebApi("TootTally");
            }
            else
            {
                Plugin.LogWarning("SteamManager not initialized, cannot obtain Steam Auth ticket.");
            }
        }

        public static void Cleanup()
        {
            SteamUser.CancelAuthTicket(hAuthTicket);
            m_GetAuthSessionTicketResponse?.Dispose();
        }
    }
}
