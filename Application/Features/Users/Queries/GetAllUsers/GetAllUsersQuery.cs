using Application.Features.Users.Dtos;
using Core.Utilities.Results;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQuery : IRequest<Response<List<GetAllUsersDto>>>
    {
    }
}
