using Ats.Api.Enums;
using Ats.Api.Models;

namespace Ats.Api.Data;

public static class DatabaseSeeder
{
    public static readonly Guid RecruiterAliceId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid HiringManagerBobId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid RecruiterCarolId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.TeamMembers.Any()) return;

        db.TeamMembers.AddRange(
            new TeamMember
            {
                Id = RecruiterAliceId,
                Name = "Alice Nguyen",
                Email = "alice@ats.dev",
                Role = TeamMemberRole.Recruiter
            },
            new TeamMember
            {
                Id = HiringManagerBobId,
                Name = "Bob Smith",
                Email = "bob@ats.dev",
                Role = TeamMemberRole.HiringManager
            },
            new TeamMember
            {
                Id = RecruiterCarolId,
                Name = "Carol James",
                Email = "carol@ats.dev",
                Role = TeamMemberRole.Recruiter
            }
        );

        await db.SaveChangesAsync();
    }
}
