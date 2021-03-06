﻿using System;
using Shamyr.MongoDB;
using Shamyr.MongoDB.Attributes;
using Shamyr.MongoDB.Indexes;

namespace Shamyr.Cloud.Database.Documents
{
  [Collection(nameof(DbCollections.Users))]
  public class UserDoc: DocumentBase
  {
    [Index(Unique = true, Sparse = true)]
    public string? Username { get; set; } = default!;

    [Index(Unique = true, Sparse = true)]
    public string? NormalizedUsername { get; set; } = default!;

    [Index(Unique = true)]
    public string NormalizedEmail { get; set; } = default!;

    [Index(Unique = true)]
    public string Email { get; set; } = default!;

    public bool Verified { get; set; }
    public string? GivenName { get; set; } = default!;
    public string? FamilyName { get; set; } = default!;
    public SecretDoc? Secret { get; set; } = default!;
    public string? EmailToken { get; set; }
    public string? PasswordToken { get; set; }
    public bool Disabled { get; set; }
    public TokenDoc? RefreshToken { get; set; }
    public DateTime? LogoutUtc { get; set; }
    public bool Admin { get; set; }
  }
}
