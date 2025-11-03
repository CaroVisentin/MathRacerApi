using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathRacerAPI.Domain.Models;


namespace MathRacerAPI.Domain.Repositories
{
    public interface IFriendshipRepository
    {
 
        /// <summary>
        /// Obtiene una relación de amistad entre dos jugadores.
        /// </summary>
        Task<Friendship?> GetFriendshipAsync(int playerId1, int playerId2);

        /// <summary>
        /// Agrega una nueva amistad.
        /// </summary>
        Task AddFriendshipAsync(Friendship friendship);

        /// <summary>
        /// Actualiza una amistad existente.
        /// </summary>
        Task UpdateFriendshipAsync(Friendship friendship);

        /// <summary>
        /// Obtiene todas las amistades aceptadas de un jugador.
        /// </summary>
        Task<IEnumerable<PlayerProfile>> GetFriendsAsync(int playerId);

        /// <summary>
        /// Obtiene un estado de solicitud por nombre (Pendiente, Aceptada, Rechazada).
        /// </summary>
        Task<RequestStatus> GetRequestStatusByNameAsync(string statusName);

        Task<IEnumerable<PlayerProfile>> GetPendingFriendRequestsAsync(int playerId);
    }

}


