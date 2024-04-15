using System.Collections.Generic;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    private const string PLAYER_ID_PREFIX ="Player ";
    private static Dictionary<string, PlayerAttributes> Dictionary = new();

    public static void RegisterPlayer(string _clientID, PlayerAttributes _player)
    {
        string _playerID = PLAYER_ID_PREFIX + _clientID;
        Dictionary.Add(_playerID,_player);
        _player.transform.name = _playerID;
    }

    public static void DeRegisterPlayer(string _clientID)
    {
        string _playerID = PLAYER_ID_PREFIX + _clientID;
        Dictionary.Remove(_playerID);
    }

    public static PlayerAttributes GetPlayer(string _OwnerClientId)
    {
        return Dictionary[_OwnerClientId];
    }
}
