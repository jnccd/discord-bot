using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WarframeNET
{
    /// <summary>
    /// In-game Dark Sector
    /// </summary>
    public class DarkSector
    {
        /// <summary>
        /// Id of the dark sector.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Type of the rail.
        /// </summary>
        public string RailType { get; set; }

        /// <summary>
        /// Is controlled by an alliance?
        /// </summary>
        public bool? IsAlliance { get; set; }

        /// <summary>
        /// Name of the deployer.
        /// </summary>
        public string DeployerName { get; set; }
        
        /// <summary>
        /// Clan of the deployer.
        /// </summary>
        public string DeployerClan { get; set; }

        /// <summary>
        /// Name of the defender.
        /// </summary>
        public string DefenderName { get; set; }

        /// <summary>
        /// MOTD of the defender.
        /// </summary>
        public string DefenderMOTD { get; set; }

        /// <summary>
        /// Who set the battle pay.
        /// </summary>
        public string BattlePaySetBy { get; set; }

        /// <summary>
        /// What clan set the battle pay.
        /// </summary>
        public string BattlePaySetByClan { get; set; }

        /// <summary>
        /// Who changed the tax lately.
        /// </summary>
        public string TaxChangedBy { get; set; }

        /// <summary>
        /// What clan changed the tax lately.
        /// </summary>
        public string TaxChangedByClan { get; set; }

        /// <summary>
        /// Mission of the dark sector's planet.
        /// </summary>
        public Mission Mission { get; set; }

        /// <summary>
        /// Battle history for the dark sector.
        /// </summary>
        public List<DarkSectorBattle> History { get; set; }

        internal DarkSector() { }
    }

    /// <summary>
    /// Battle that occured in a dark sector.
    /// </summary>
    public class DarkSectorBattle
    {
        /// <summary>
        /// Who defended.
        /// </summary>
        public string Defender { get; set; }

        /// <summary>
        /// Were the defenders an alliance?
        /// </summary>
        [JsonProperty("defenderIsAlliance")]
        public bool? IsDefenderAlliance { get; set; }

        /// <summary>
        /// Who attacked.
        /// </summary>
        public string Attacker { get; set; }

        /// <summary>
        /// Were the attackers an alliance.
        /// </summary>
        [JsonProperty("attackerIsAlliance")]
        public bool? IsAttackerAlliance { get; set; }

        /// <summary>
        /// Who won the battle
        /// </summary>
        public string Winner { get; set; }

        /// <summary>
        /// Battle start time.
        /// </summary>
        [JsonProperty("start")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Battle end time.
        /// </summary>
        [JsonProperty("end")]
        public DateTime EndTime { get; set; }

        internal DarkSectorBattle() { }
    }
}