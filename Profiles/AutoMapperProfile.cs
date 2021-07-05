using AutoMapper;
using System.Collections.Generic;
using Trading.Data.Entities;
using Trading.Dtos;
using Trading.Shared.Results;

namespace Trading.WebApi.Profiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Group, GroupDto>();
            CreateMap<OperationResult<Group>, OperationResult<GroupDto>>();
            CreateMap<OperationResult<List<Group>>, OperationResult<List<GroupDto>>>();

            CreateMap<UserGroup, UserGroupDto>();
            CreateMap<OperationResult<UserGroup>, OperationResult<UserGroupDto>>();
            CreateMap<OperationResult<List<UserGroup>>, OperationResult<List<UserGroupDto>>>();

            CreateMap<ApplicationUser, UserDto>();
            CreateMap<OperationResult<ApplicationUser>, OperationResult<UserDto>>();
            CreateMap<OperationResult<List<ApplicationUser>>, OperationResult<List<UserDto>>>();

            CreateMap<Application, ApplicationDto>();
            CreateMap<OperationResult<Application>, OperationResult<ApplicationDto>>();
            CreateMap<OperationResult<List<Application>>, OperationResult<List<ApplicationDto>>>();

            CreateMap<ApplicationDto, Application>();
            CreateMap<ApplicationFileDto, ApplicationFile>();

            CreateMap<ApplicationFile, ApplicationFileDto>();
            CreateMap<OperationResult<ApplicationFile>, OperationResult<ApplicationFileDto>>();
            CreateMap<OperationResult<List<ApplicationFile>>, OperationResult<List<ApplicationFileDto>>>();

            CreateMap<Terminal, TerminalDto>();
            CreateMap<Terminal, TerminalInVmDto>();
            CreateMap<OperationResult<Terminal>, OperationResult<TerminalDto>>();
            CreateMap<OperationResult<List<Terminal>>, OperationResult<List<TerminalDto>>>();

            CreateMap<VirtualMachine, VirtualMachineDto>();
            CreateMap<CreateVirtualMachineDto, VirtualMachine>();
            CreateMap<OperationResult<VirtualMachine>, OperationResult<VirtualMachineDto>>();
            CreateMap<OperationResult<List<VirtualMachine>>, OperationResult<List<VirtualMachineDto>>>();

            CreateMap<DefaultTerminal, Terminal>();
            CreateMap<DefaultApplication, Application>();
            CreateMap<DefaultApplicationFile, ApplicationFile>();
        }
    }
}