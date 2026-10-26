using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartProd.API.Server.Data;

#nullable disable

namespace SmartProd.API.Server.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261026162000_AddUsuarioRefreshTokens")]
partial class AddUsuarioRefreshTokens
{
}
