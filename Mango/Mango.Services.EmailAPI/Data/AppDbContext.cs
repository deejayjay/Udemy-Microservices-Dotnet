﻿using Microsoft.EntityFrameworkCore;
using Mango.Services.EmailAPI.Models;

namespace Mango.Services.EmailAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<EmailLogger> EmailLoggers { get; set; }
}
