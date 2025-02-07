﻿using Microsoft.AspNetCore.Http;

namespace MeetingApp.Core.DTOs;

public record UserDto(int Id, string Name, string Surname, string Email, string Phone, string Password, string ProfileImagePath);