using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class LobbyScript : MonoBehaviour
{
    public bool isLobbyPrivate;
    public string lobbyName = "My Lobby";
    public int maxPlayers = 5;
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer = 15f;
    private float pollUpdateTimer = 1.1f;
    private string playerName = "sckapa";


    async void Start()
    {
        //Start unity service
        await UnityServices.InitializeAsync();

        //try disabling this
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        //Sign in
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    #region Lobby
    public async void CreateLobby()
    {
        try
        {
            if (isLobbyPrivate == false)
            {
                CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Player = new Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
                        }
                    },
                    Data = new Dictionary<string, DataObject>
                    {
                        {"GAME_MODE", new DataObject(DataObject.VisibilityOptions.Public, "Capture The Flag")},
                        {"RELAY_CODE", new DataObject(DataObject.VisibilityOptions.Public, "0")}
                    }
                };

                hostLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
                joinedLobby = hostLobby;
                Debug.Log("Lobby created with name : " + hostLobby.Name + " and code :" + hostLobby.LobbyCode);
                PrintPlayers(hostLobby);
            }
        }

        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void UpdateLobbyGameMode(string newGameMode)
    {
        try
        {
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {"GAME_MODE", new DataObject(DataObject.VisibilityOptions.Public, newGameMode)}
                }
            };

            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, updateLobbyOptions);
            joinedLobby = hostLobby;
            PrintPlayers(joinedLobby);
        }

        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void ListLobbiesWithFilter()
    {
        try
        {
            //Create a query dictionary
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(queryResponse.Results.Count + " Lobbies found : " + lobby.Name);
            }

        }

        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    public async void JoinLobby(string _lobbyCode)
    {
        try
        {
            _lobbyCode = _lobbyCode.ToUpper();
            joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(_lobbyCode);
            Debug.Log("Joined lobby with code : " + _lobbyCode);
        }

        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            await Lobbies.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }

        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void IsHost()
    {

    }

    public void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }

    private void PrintPlayers(Lobby lobby)
    {
        foreach (Player player in lobby.Players)
        {
            Debug.Log("Players in lobby : " + player.Data["PlayerName"].Value + " " + hostLobby.Data["GAME_MODE"].Value);
        }
    }

    private async void HeartbeatHandler()
    {
        float heartbeatTimerMax = 15f;
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0f)
            {
                heartbeatTimer = heartbeatTimerMax;

                //Ping server to keep lobby alive
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPollForUpdateAsync()
    {
        float pollUpdateTimerMax = 1.1f;
        if (joinedLobby != null)
        {
            pollUpdateTimer -= Time.deltaTime;
            if (pollUpdateTimer <= 0f)
            {
                pollUpdateTimer = pollUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
            }

            if (joinedLobby.Data["RELAY_CODE"].Value != "0")
            {
                JoinRelay(joinedLobby.Data["RELAY_CODE"].Value);
            }
        }
    }

    public async void StartGame()
    {
        try
        {
            string lobbyCode = await CreateRelay();
            Lobby lobby = await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {"RELAY_CODE", new DataObject(DataObject.VisibilityOptions.Member, lobbyCode)}
                }
            });
            joinedLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    #endregion

    #region Relay
    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string lobbyCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
            return lobbyCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    public async void JoinRelay(string _lobbyCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(_lobbyCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
    #endregion
    private void Update()
    {
        HeartbeatHandler();
        HandleLobbyPollForUpdateAsync();
    }
}
