using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zrpg.Game;

namespace GUI.Pages.HeroSubPages
{
    class MapTabCell
    {
        HeroesPage heroesPage;
        string heroId;

        public MapTabCell(HeroesPage heroesPage, string heroId)
        {
            this.heroesPage = heroesPage;
            this.heroId = heroId;
        }

        public HeroesPage HeroesPage { get { return heroesPage; } }
        public string HeroId { get { return heroId; } }
    }
}
