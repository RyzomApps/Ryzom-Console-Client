///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

// ReSharper disable InconsistentNaming
#pragma warning disable CA1069
namespace Client.Brick;

public enum BrickFamily
{
    // COMBAT
    // ********
    BeginCombat = 0,
    // ROOT COMBAT
    BeginCombatRoot = BeginCombat,
    BFPA = BeginCombatRoot,
    EndCombatRoot = BFPA,

    // MANDATORY COMBAT
    // BeginCombatMandatory,
    // EndCombatMandatory = ???,

    // OPTION COMBAT
    BeginCombatOption,
    BFOA = BeginCombatOption,
    BFOB,
    BFOC,
    BFOD,
    BFOE,
    BFOF,
    EndCombatOption = BFOF,

    // PARAMETER COMBAT
    BeginCombatParameter,
    BFMA = BeginCombatParameter,
    BFMB,
    BFMC,
    BFMD,
    BFME,
    BFMF,
    BFMG,
    BFMH,
    BFMRF, // Range dmg Fire
    BFMRT, // Range dmg Poison
    BFMRW, // Range dmg Shockwave
    BFMRE, // Range dmg Electricity

    BFHME,
    BFAHHME,
    BFAHCME,
    BFAHAME,
    BFAHHAME,
    BFAHLME,
    BFAHFME,
    BFKME,
    BFAKHME,
    BFAKCME,
    BFAKAME,
    BFAKHAME,
    BFAKLME,
    BFAKFME,
    BFQME,
    BFAQHME,
    BFAQCME,
    BFAQAME,
    BFAQHAME,
    BFAQLME,
    BFAQFME,

    // deprecated: only used in saibricks
    BFM1MC,
    BFM2MC,
    BFM1HMC,
    BFM1PMC,
    BFM1BMC,
    BFM1SMC,
    BFM2PMC,
    BFM2BMC,
    BFM2SMC,
    BFR1MC,
    BFR2MC,
    BFM1MD,
    BFM2MD,
    BFR1MD,
    BFR2MD,
    BFM1ME,
    BFM2ME,
    BFR1ME,
    BFR2ME,
    BFM1MF,
    BFM2MF,
    BFR1MF,
    BFR2MF,
    BFM1BMG,
    BFM2BMG,
    BFM2SMG,
    BFM1BMH,
    BFM2BMH,
    BFM2SMH,
    BFM1BMI,
    BFM2BMI,
    BFM1SMJ,
    BFM1PMK,
    BFM2PMK,
    BFR2LFML,
    BFM2SSFML,
    BFM2SAFML,
    BFM1SAFML,
    BFM1BMTMM,
    BFM2BMTMM,
    BFR1HTMM,
    BFR1BTMM,
    BFM1BSZMN,
    BFM1PSZMN,
    BFR2BZMN,
    BFM2PPZMN,
    BFM1PDMMO,
    BFR1PMMO,
    BFM1SSMMO,
    BFR2RMMO,
    BFMMQ,
    BFMR,
    BFMK,
    BFMP,
    // end deprecated
    EndCombatParameter = BFMP,

    // CREDIT COMBAT
    BeginCombatCredit,
    BFCA = BeginCombatCredit,
    BFCB,
    BFCC,
    BFCD,
    BFCE,
    BFCF,
    BFCG,
    EndCombatCredit = BFCG,
    EndCombat = EndCombatCredit,

    // MAGIC
    // ********
    BeginMagic,
    // ROOT MAGIC
    BeginMagicRoot = BeginMagic,
    BMPA = BeginMagicRoot,
    EndMagicRoot = BMPA,

    // MANDATORY MAGIC
    BeginMagicMandatory,
    BMDALEA = BeginMagicMandatory,
    BMDHTEA,
    BMOALEA,
    BMOELEA,
    BMOETEA,
    BMSTEA,
    EndMagicMandatory = BMSTEA,

    // OPTION MAGIC
    BeginMagicOption,
    BMOF = BeginMagicOption,
    BMOG,
    BMOH,
    BMOR,
    BMOV,
    EndMagicOption = BMOV,

    // PARAMETER MAGIC
    BeginMagicParameter,
    BMTODMB = BeginMagicParameter,
    BMDALMF,
    BMDALMM,
    BMDALMS,
    BMDHTMA,
    BMDHTMP,
    BMDHTMT,
    BMOALMA,
    BMOALMB,
    BMOALMD,
    BMOALMM,
    BMOALMR,
    BMOELMA,
    BMOELMC,
    BMOELME,
    BMOELMF,
    BMOELMP,
    BMOELMR,
    BMOELMS,
    BMOETMA,
    BMOETMC,
    BMOETME,
    BMOETMF,
    BMOETMP,
    BMOETMR,
    BMOETMS,
    BMSTMA,
    BMSTMC,
    BMSTMP,
    BMSTMT,
    EndMagicParameter = BMSTMT,

    // CREDIT MAGIC
    BeginMagicCredit,
    BMCA = BeginMagicCredit,
    BMCC,
    BMCP,
    BMCR,
    EndMagicCredit = BMCR,
    EndMagic = EndMagicCredit,

    // FABER
    // ********
    BeginFaber,
    // ROOT FABER
    BeginFaberRoot = BeginFaber,
    BCPA = BeginFaberRoot,
    EndFaberRoot = BCPA,

    // MANDATORY FABER
    BeginFaberMandatory,
    BCCMEA = BeginFaberMandatory,
    BCCREA,
    BCCPEA,
    BCCAEA,
    BCCAEB,
    BCCAEC,
    BCCAED,
    BCCSEA,
    BCBMEA,
    BCBREA,
    BCFMEA,
    BCFREA,
    BCFPEA,
    BCFAEA,
    BCFAEB,
    BCFAEC,
    BCFAED,
    BCFSEA,
    BCFJEA,
    BCMMEA,
    BCMREA,
    BCMPEA,
    BCMAEA,
    BCMAEB,
    BCMAEC,
    BCMAED,
    BCMSEA,
    BCMJEA,
    BCTMEA,
    BCTREA,
    BCTPEA,
    BCTAEA,
    BCTAEB,
    BCTAEC,
    BCTAED,
    BCTSEA,
    BCTJEA,
    BCZMEA,
    BCZREA,
    BCZPEA,
    BCZAEA,
    BCZAEB,
    BCZAEC,
    BCZAED,
    BCZSEA,
    BCZJEA,
    BCRMEA,
    BCRAEA,
    BCKAMMI,
    BCKARMI,
    BCOKAMM01,
    BCOKAMR01,
    BCOKAMT01,
    BCOKARM01,
    BCOKARR01,
    BCOKART01,
    BCOKAMM02,
    BCOKAMR02,
    BCOKAMT02,
    BCOKARM02,
    BCOKARR02,
    BCOKART02,
    BCOMARM01,
    BCOMARR01,
    BCOMART01,
    BCOMARM02,
    BCOMARR02,
    BCOMART02,
    BCOGENM01,
    BCOGENR01,
    BCOGENT01,
    BCOGENM02,
    BCOGENR02,
    BCOGENT02,

    EndFaberMandatory = BCOGENT02,

    // OPTION FABER
    BeginFaberOption,
    BCOA = BeginFaberOption,
    BCOB,
    BCOC,
    BCOD,
    BCFAOA,
    BCFMOA,
    BCFROA,
    BCFPOA,
    BCFSOA,
    BCFJOA,
    BCMAOA,
    BCMMOA,
    BCMROA,
    BCMPOA,
    BCMSOA,
    BCMJOA,
    BCTAOA,
    BCTMOA,
    BCTROA,
    BCTPOA,
    BCTSOA,
    BCTJOA,
    BCZAOA,
    BCZMOA,
    BCZROA,
    BCZPOA,
    BCZSOA,
    BCZJOA,
    BCFAOB,
    BCFMOB,
    BCFROB,
    BCFPOB,
    BCFSOB,
    BCFJOB,
    BCMAOB,
    BCMMOB,
    BCMROB,
    BCMPOB,
    BCMSOB,
    BCMJOB,
    BCTAOB,
    BCTMOB,
    BCTROB,
    BCTPOB,
    BCTSOB,
    BCTJOB,
    BCZAOB,
    BCZMOB,
    BCZROB,
    BCZPOB,
    BCZSOB,
    BCZJOB,
    BCFAOC,
    BCFMOC,
    BCFROC,
    BCFPOC,
    BCFSOC,
    BCFJOC,
    BCMAOC,
    BCMMOC,
    BCMROC,
    BCMPOC,
    BCMSOC,
    BCMJOC,
    BCTAOC,
    BCTMOC,
    BCTROC,
    BCTPOC,
    BCTSOC,
    BCTJOC,
    BCZAOC,
    BCZMOC,
    BCZROC,
    BCZPOC,
    BCZSOC,
    BCZJOC,
    BCFAOD,
    BCFMOD,
    BCFROD,
    BCFPOD,
    BCFSOD,
    BCFJOD,
    BCMAOD,
    BCMMOD,
    BCMROD,
    BCMPOD,
    BCMSOD,
    BCMJOD,
    BCTAOD,
    BCTMOD,
    BCTROD,
    BCTPOD,
    BCTSOD,
    BCTJOD,
    BCZAOD,
    BCZMOD,
    BCZROD,
    BCZPOD,
    BCZSOD,
    BCZJOD,
    EndFaberOption = BCZJOD,

    // CREDIT FABER
    BeginFaberCredit,
    BCFACA = BeginFaberCredit,
    BCFMCA,
    BCFRCA,
    BCFPCA,
    BCFSCA,
    BCFJCA,
    BCCMCA,
    BCCRCA,
    BCCPCA,
    BCMACA,
    BCMMCA,
    BCMRCA,
    BCMPCA,
    BCMSCA,
    BCMJCA,
    BCTACA,
    BCTMCA,
    BCTRCA,
    BCTPCA,
    BCTSCA,
    BCTJCA,
    BCZACA,
    BCZMCA,
    BCZRCA,
    BCZPCA,
    BCZSCA,
    BCZJCA,
    BCKAMBCA,
    BCKARBCA,
    BCFTCA,
    BCMTCA,
    BCTTCA,
    BCZTCA,
    EndFaberCredit = BCZTCA,

    // MISC FABER
    BeginFaberRawMaterial,
    FARawMaterial = BeginFaberRawMaterial,
    EndFaberRawMaterial = FARawMaterial,

    BeginFaberTool,
    FATool = BeginFaberTool,
    EndFaberTool = FATool,
    EndFaber = EndFaberTool,

    // FORAGE PROSPECTION
    // ******************
    BeginForageProspection,
    BeginForageProspectionRoot = BeginForageProspection,
    BHFPPA = BeginForageProspectionRoot,
    BHFSPA,
    BHFGPA,
    EndForageProspectionRoot = BHFGPA,

    BeginForageProspectionOption,
    BHFPPOA = BeginForageProspectionOption,
    BHFPPOB,
    BHFPPOC,
    BHFPPOD,
    BHFPPOE,
    BHFPPOF,
    BHFPPOG,
    BHFPPOH,
    BHFPPOI,
    BHFPPOJ,
    BHFPPOK,
    BHFPPOL,
    BHFPSOA,
    EndForageProspectionOption = BHFPSOA,

    BeginForageProspectionParameter,
    BHFPMA = BeginForageProspectionParameter,
    BHFPMB,
    BHFPRMFMA,
    BHFPRMFMB,
    BHFPRMFMC,
    BHFPRMFMD,
    BHFPRMFME,
    BHFPRMFMF,
    BHFPRMFMG,
    BHFPRMFMH,
    BHFPRMFMI,
    BHFPRMFMJ,
    BHFPRMFMK,
    BHFPRMFML,
    BHFPRMFMM,
    BHFPRMFMN,
    BHFPRMFMO,
    BHFPRMFMP,
    BHFPRMFMQ,
    BHFPRMFMR,
    BHFPRMFMS,
    BHFPRMFMT,
    BHFPRMFMU,
    BHFPRMFMV,
    BHFPRMFMW,
    BHFPRMFMX,
    BHFPMC,
    BHFPMD,
    BHFPME,
    BHFPMF,
    BHFPMG,
    BHFPMH,
    BHFPMI,
    BHFPMJ,
    BHFPMK,
    BHFPML,
    BHFPMM,
    EndForageProspectionParameter = BHFPMM,

    BeginForageProspectionCredit,
    BHFPCA = BeginForageProspectionCredit,
    EndForageProspectionCredit = BHFPCA,

    EndForageProspection = EndForageProspectionCredit,

    // FORAGE EXTRACTION
    // *****************
    BeginForageExtraction,
    BeginForageExtractionRoot = BeginForageExtraction,
    BHFEPA = BeginForageExtractionRoot,
    EndForageExtractionRoot = BHFEPA,

    BeginForageExtractionOption,
    BHFEOA = BeginForageExtractionOption,
    BHFEOB,
    BHFEOC,
    BHFEOD,
    BHFEOE,
    BHFEOF,
    BHFEOG,
    BHFEOH,
    EndForageExtractionOption = BHFEOH,

    BeginForageExtractionMandatory,
    BHFEEA = BeginForageExtractionMandatory,
    BHFEEB,
    BHFEEC,
    EndForageExtractionMandatory = BHFEEC,

    BeginForageExtractionParameter,
    BHFEMA = BeginForageExtractionParameter,
    BHFEMB,
    BHFEMC,
    BHFEMD,
    BHFEME,
    BHFEMF,
    BHFEMG,
    BHFEMK,
    EndForageExtractionParameter = BHFEMK,

    BeginForageExtractionCredit,
    BHFECA = BeginForageExtractionCredit,
    EndForageExtractionCredit = BHFECA,

    EndForageExtraction = EndForageExtractionCredit,

    // HARVEST
    // ********
    BeginHarvest,
    BeginHarvestRoot = BeginHarvest,
    RootHarvest,
    EndHarvestRoot = RootHarvest,
    EndHarvest = EndHarvestRoot,

    // TRAINING
    // ********
    BeginTraining,
    BTFOC = BeginTraining,
    BTHP,
    BTSAP,
    BTSTA,

    // special for charac buying
    BeginCharacBuy,
    BPPC = BeginCharacBuy,
    BPPM,
    BPPI,
    BPPW,
    BPPS,
    BPPB,
    BPPD,
    BPPL,
    EndCharacBuy = BPPL,

    EndTraining = EndCharacBuy,

    // Bonuses
    BeginBonus,
    BPBCA = BeginBonus, // Craft: Durability Bonus
    BPBHFEA, // Harvest Forage Extraction: Time Bonus
    BPBGLA, // Generic Landmark: Extender Bonus
    EndBonus = BPBGLA,

    // TITLE
    // ******
    BeginTitle,
    BPTEA = BeginTitle,
    EndTitle = BPTEA,

    // SPECIAL FOR INTERFACE (no interaction)
    // ********
    BeginInterface,
    BIF = BeginInterface, // Interface for Fight Action representation
    BIG, // Interface General bricks.
    EndInterface = BIG,

    // FOR SPECIAL POWERS
    // ********
    BeginSpecialPowers,
    // root power
    BeginPowerRoot = BeginSpecialPowers,
    BSXPA = BeginSpecialPowers, // root power/aura
    EndPowerRoot = BSXPA,

    // mandatory power
    BeginPowerMandatory,
    BSXEA = BeginPowerMandatory, // power
    BSXEB, // aura
    BSCEA, // consumable power
    EndPowerMandatory = BSCEA,

    // parameters power
    BeginPowerParameter,
    BeginFightPowerParameter = BeginPowerParameter,
    BSFMA = BeginFightPowerParameter, // taunt power
    BSFMB, // shielding power
    BSFMC, // stamina aura
    BSFMD, // protection aura
    BSFME, // umbrella aura
    BSFMF, // berserk
    BSFMG, // war cry
    BSFMH, // heal stamina
    BSFMI, // fire wall
    BSFMJ, // thorn wall
    BSFMK, // water wall
    BSFML, // lightning
    EndFightPowerParameter = BSFML,

    BSXMA, // life aura
    BSXMB, // invulnerability
    BSXMC, // heal Hp

    BSDMA, // speed

    // G for general ??
    BSGMA, // heal focus
    BSGMB, // enchant weapon
    BSGMBA,
    BSGMBC,
    BSGMBE,
    BSGMBF,
    BSGMBP,
    BSGMBR,
    BSGMBS,

    BeginMagicPowerParameter,
    BSMMA = BeginMagicPowerParameter, // sap aura
    BSMMB, // anti magic shield
    BSMMC, // balance hp
    BSMMD, // heal sap
    EndMagicPowerParameter = BSMMD,

    // consumable powers
    BSCMA, // heal Hp
    BSCMB, // heal Sap
    BSCMC, // heal Sta
    BSCMD, // heal Focus
    EndPowerParameter = BSCMD,

    BSGMC, // allegories
    BSGMCB, // boost allegories

    BeginMagicPowerCredit,
    BSXCA = BeginMagicPowerCredit, // recast time
    EndMagicPowerCredit = BSXCA,

    EndSpecialPowers = EndMagicPowerCredit,

    // FOR TIMED ACTIONS
    // ********
    BeginTimedActions,
    BAPA = BeginTimedActions,
    EndTimedActions = BAPA,

    /* If you add a new brick Type, you should change
         isRootFamily(), isMandatoryFamily(), isOptionFamily(), isCreditFamily()
         brickType()
         */
    BeginProcEnchantement,
    BEPA = BeginProcEnchantement,
    EndProcEnchantement = BEPA,

    NbFamilies,
    Unknown,

    // Yoyo: just for code below work (isRoot etc....). remove entries when true families are described
    AutoCodeCheck,

    BeginCombatMandatory = AutoCodeCheck,
    EndCombatMandatory = AutoCodeCheck,

    BeginFaberParameter = AutoCodeCheck,
    EndFaberParameter = AutoCodeCheck,

    BeginHarvestMandatory = AutoCodeCheck,
    EndHarvestMandatory = AutoCodeCheck,
    BeginHarvestOption = AutoCodeCheck,
    EndHarvestOption = AutoCodeCheck,
    BeginHarvestCredit = AutoCodeCheck,
    EndHarvestCredit = AutoCodeCheck,
    BeginHarvestParameter = AutoCodeCheck,
    EndHarvestParameter = AutoCodeCheck,

    BeginForageProspectionMandatory = AutoCodeCheck,
    EndForageProspectionMandatory = AutoCodeCheck
}