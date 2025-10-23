using CherAmiAPI.Interfaces.Service;
using CrazyLizard.Contexts;
using CrazyLizard.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrazyLizard.Services
{
    public class inviteCodeService(ApplicationDbContext ctx) : IInviteCodeService
    {
        public async Task<string> GenerateCodeAsync()
        {
            List<string> adjectives = await ctx.Words.
                                      Where(w => w.Type == Word.WordType.Adjective).
                                      Select(w => w.Text).
                                      ToListAsync();

            List<string> nouns = await ctx.Words.
                                       Where(w => w.Type == Word.WordType.Noun).
                                       Select(w => w.Text).
                                       ToListAsync();

            bool codeUnique = false;
            Random random = new();
            string randomAdjective;
            string randomNoun;
            string potentialCode = "";

            while (!codeUnique)
            {
                randomAdjective = adjectives[random.Next(adjectives.Count)];
                randomNoun = nouns[random.Next(nouns.Count)];

                potentialCode = char.ToUpper(randomAdjective[0]) + randomAdjective.Substring(1) + char.ToUpper(randomNoun[0]) + randomNoun.Substring(1);

                codeUnique = !await ctx.Circles.AnyAsync(c => c.CircleCode == potentialCode);
            }

            return potentialCode;
        }
    }
}
