﻿using Microsoft.AspNetCore.Http;

namespace MeetingApp.Core.DTOs.UpdateDto;

public record UpdateUserDto(string? Name, string? Surname, string? Email, string? Phone, string? Password, IFormFile? ProfileImageFile, string? ProfileImagePath);