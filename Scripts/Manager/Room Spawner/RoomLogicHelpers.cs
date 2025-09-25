using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace _Lofty.Hidden.Helpers
{
    public static class RoomLogicHelpers
    {
        /// <summary>
        /// the method to check if the room is fully block in every direction around the room
        /// </summary>
        /// <param name="_room"></param> the target room to find is the room is fully blocked?
        /// <returns></returns>
        public static bool IsRoomFullyBlocked(Room _room, RoomManager _manager)
        {
            // Check if the room is fully blocked by hard or soft blocks
            foreach (var _direction in _manager.RoomSpawnDirections)
            {
                var _checkPosition = Vector3Int.FloorToInt(_room.transform.position + _direction);

                if (!_manager.HardBlockPositions.ContainsKey(_checkPosition)
                && !_manager.SoftBlockPositions.ContainsKey(_checkPosition))
                {
                    return false; // Found an empty position, room is not fully blocked
                }
            }

            return true; // All positions are blocked
        }

        /// <summary>
        /// the method to get the fallback room path if the current room is blocked
        /// This method will find the neighbor room and check if the position is blocked or not,
        /// </summary>
        /// <param name="_tempRoom"></param> the temporary room to find the fallback path
        /// <param name="_fallbackPosition"></param> the fallback position to return if the method finds a valid path
        /// <returns></returns>
        public static bool GetFallbackRoomPath(ref Room _tempRoom, RoomManager _manager, out Vector3 _fallbackPosition)
        {
            var _oldRoom = _tempRoom;
            var _basedRoom = _tempRoom?.BasedRoom;
            if (_basedRoom == null || _basedRoom.Neighbor.Count == 0)
            {
                _fallbackPosition = Vector3.zero;
                return false;
            }

            _tempRoom = _basedRoom; // Fallback to the based room

            foreach (var _neighbor in _tempRoom.Neighbor)
            {
                if (_neighbor == _oldRoom) continue; // Skip the old room

                foreach (var _direction in _manager.RoomSpawnDirections)
                {
                    var _newDirection = _neighbor.transform.position + _direction;

                    var _isPositionBlocked = _manager.HardBlockPositions.ContainsKey(Vector3Int.FloorToInt(_newDirection))
                        || _manager.IsPositionHaveBlocked(_newDirection, _neighbor, BlockType.HardBlock)
                        || _manager.IsPositionHaveBlocked(_newDirection, _neighbor, BlockType.SoftBlock)
                        || !_manager.SoftBlockPositions.TryGetValue(Vector3Int.FloorToInt(_newDirection), out var _roomValue)
                        || _roomValue != _neighbor;

                    if (_isPositionBlocked)
                    {
                        continue; // Position is blocked or not a valid soft block
                    }

                    _tempRoom = _neighbor; // Found a valid fallback room
                    _fallbackPosition = _newDirection; // return fallback position out parameter
                    return true; // Exit after finding the first valid neighbor
                }
            }

            _fallbackPosition = Vector3.zero; // No valid fallback position
            return false;
        }
    }
}
