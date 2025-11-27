
using System.Security.Permissions;
using MoreSlugcats;
using UnityEngine;
using RWCustom;
using System.Reflection;
using LizardCosmetics;
// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Harbinger;

/// <summary>
/// Static class that handles zap lizard stuff
/// </summary>
public static class ZapLizardStuff
{
    static CreatureTemplate ZaplizTemp;
    static LizardBreedParams ZaplizParams;
    public static void InitLizor()
    {
        On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate += AddZapLorsor;
        On.LizardTongue.ctor += LizardTongue_ctor;
        On.LizardTongue.Retract += LizardTongue_Retract_Hook;
        On.Lizard.ctor += Lizard_ctor;
        On.LizardGraphics.ctor += LizardGraphics_ctor;
    }

    private static void LizardGraphics_ctor(On.LizardGraphics.orig_ctor orig, LizardGraphics self, PhysicalObject ow)
    {
        orig(self, ow);

        if (self.lizard.lizardParams == ZaplizParams)
        {
            self.ivarBodyColor = new HSLColor(UnityEngine.Random.Range(0.1718f, 0.246f), UnityEngine.Random.Range(0.7f, 1.0f), UnityEngine.Random.Range(0.9f, 1f)).rgb;
            self.AddCosmetic(self.TotalSprites, new JumpRings(self, self.TotalSprites));
        }
    }

    private static void Lizard_ctor(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
    {
        if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.YellowLizard && abstractCreature.unrecognizedFlags.Contains("ZapLizard"))
        {
            abstractCreature.creatureTemplate = ZaplizTemp;
            orig(self, abstractCreature, world);
            abstractCreature.personality.dominance = 1f;
        }
        else
        {
            orig(self, abstractCreature, world);
        }
    }

    private static void LizardTongue_Retract_Hook(On.LizardTongue.orig_Retract orig, LizardTongue self)
    {
        if (self.lizard.Template.type == CreatureTemplate.Type.YellowLizard && self.attached != null)
        {
            self.lizard.room.PlaySound(SoundID.Centipede_Shock, self.lizard.mainBodyChunk);

            for (int i = 0; i < 8f; i++)
            {
                self.lizard.room.AddObject(new Spark(self.pos, Custom.RNV() * Mathf.Lerp(4f, 14f, UnityEngine.Random.value), new Color(0.7f, 0.7f, 1f), null, 8, 14));
            }
            self.lizard.room.AddObject(new ZapCoil.ZapFlash(self.pos, 5f));
            if (self.attached.owner.Submersion > 0.5f)
            {
                self.lizard.room.AddObject(new UnderwaterShock(self.lizard.room, null, self.pos, 10, 800f, 2f, self.lizard, new Color(0.8f, 0.8f, 1f)));
            }
            if (self.attached.owner is Creature)
            {
                if (self.attached.owner is Player && (self.attached.owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
                {
                    (self.attached.owner as Player).PyroDeath();
                }
                else
                {
                    (self.attached.owner as Creature).Violence(self.lizard.mainBodyChunk, new Vector2?(new Vector2(0f, 0f)), (self.attached.owner as Creature).mainBodyChunk, null, Creature.DamageType.Electric, 0.5f, 20f);
                    (self.attached.owner as Creature).Stun(120);
                    self.lizard.room.AddObject(new CreatureSpasmer(self.attached.owner as Creature, false, (self.attached.owner as Creature).stun));
                    (self.attached.owner as Creature).LoseAllGrasps();
                }
            }
        }
        orig(self);
    }

    private static void LizardTongue_ctor(On.LizardTongue.orig_ctor orig, LizardTongue self, Lizard lizard)
    {
        if (lizard.Template.type == CreatureTemplate.Type.YellowLizard)
        {
            self.range = 550f;
            self.elasticRange = 0.1f;
            self.lashOutSpeed = 40f;
            self.reelInSpeed = 0.06f;
            self.chunkDrag = 0f;
            self.terrainDrag = 0f;
            self.dragElasticity = 0.05f;
            self.emptyElasticity = 0.08f;
            self.involuntaryReleaseChance = 1f;
            self.voluntaryReleaseChance = 1f;
        }
        orig(self, lizard);

    }


    public static class ObjectCopier
    {
        public static object ShallowCopy(object o)
        {
            return o?.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(o, null);
        }
    }
    private static CreatureTemplate AddZapLorsor(On.LizardBreeds.orig_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate orig, CreatureTemplate.Type type, CreatureTemplate lizardAncestor, CreatureTemplate pinkTemplate, CreatureTemplate blueTemplate, CreatureTemplate greenTemplate)
    {
        var OriginalTemp = orig(type, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
        if (type == CreatureTemplate.Type.YellowLizard)
        {
            (OriginalTemp.breedParameters as LizardBreedParams).tongue = false;
            (OriginalTemp.breedParameters as LizardBreedParams).tongueAttackRange = 500f;
            (OriginalTemp.breedParameters as LizardBreedParams).tongueWarmUp = 160;
            (OriginalTemp.breedParameters as LizardBreedParams).tongueSegments = 20;
            (OriginalTemp.breedParameters as LizardBreedParams).tongueChance = 0.1f;
            ZaplizParams = (LizardBreedParams)ObjectCopier.ShallowCopy(OriginalTemp.breedParameters as LizardBreedParams);
            ZaplizParams.tongue = true;
            ZaplizParams.tailLengthFactor = 2.5f;
            ZaplizParams.biteDamageChance = 1.0f;
            ZaplizParams.bodySizeFac = 1.25f;
            ZaplizParams.danger = 0.5f;
            ZaplizTemp = (CreatureTemplate)ObjectCopier.ShallowCopy(OriginalTemp);
            ZaplizTemp.baseDamageResistance = 0.25f;
            ZaplizTemp.breedParameters = ZaplizParams;
        }
        return OriginalTemp;
    }
}