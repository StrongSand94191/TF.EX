﻿using TF.EX.Domain.Models;
using TF.EX.Domain.Models.State;

namespace TF.EX.Domain.Ports
{
    public interface INetplayManager
    {
        void Poll();

        void Init();

        bool IsInit();

        StatusImpl AdvanceFrame(Input input);

        void UpdateNetplayRequests();

        void SaveGameState(GameState gs);

        void AdvanceGameState();

        GameState LoadGameState();

        NetworkStats GetNetworkStats();

        bool IsTestMode();

        bool IsReplayMode();

        bool IsRollbackFrame();

        void SetIsRollbackFrame(bool isRollbackFrame);

        bool IsUpdating();

        void SetIsUpdating(bool isUpdating);

        bool HaveFramesToReSimulate();

        double GetFramesToReSimulate();

        void UpdateFramesToReSimulate(int frames);

        NetplayMode GetNetplayMode();

        bool CanAdvanceFrame();

        bool HaveRequestToHandle();

        NetplayRequest ConsumeNetplayRequest();

        bool IsSynchronized();

        bool IsDisconnected();

        bool IsFramesAhead();

        bool HasDesynchronized();

        bool IsAttemptingToReconnect();

        bool IsSynchronizing();
        void Reset();
        GGRSConfig GetConfig();
        void UpdateConfig(GGRSConfig config);
        void SaveConfig();

        string GetPlayer2Name();

        void UpdatePlayer2Name(string name);
        void SetRoom(string roomUrl);

        bool ShouldSwapPlayer();
        PlayerDraw GetPlayerDraw();
        void SetPlayersIndex(int playerDraw);
        void EnableReplayMode();
        void DisableReplayMode();
    }
}
