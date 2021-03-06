#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using OpenRA.Activities;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Displays an overlay whenever resources are harvested by the actor.")]
	class WithHarvestAnimationInfo : ITraitInfo, Requires<RenderSpritesInfo>, Requires<IBodyOrientationInfo>
	{
		[Desc("Sequence name to use")]
		public readonly string Sequence = "harvest";

		[Desc("Position relative to body")]
		public readonly WVec Offset = WVec.Zero;

		public readonly string Palette = "effect";

		public object Create(ActorInitializer init) { return new WithHarvestAnimation(init.Self, this); }
	}

	class WithHarvestAnimation : INotifyHarvesterAction
	{
		WithHarvestAnimationInfo info;
		Animation anim;
		bool visible;

		public WithHarvestAnimation(Actor self, WithHarvestAnimationInfo info)
		{
			this.info = info;
			var rs = self.Trait<RenderSprites>();
			var body = self.Trait<IBodyOrientation>();

			anim = new Animation(self.World, rs.GetImage(self), RenderSimple.MakeFacingFunc(self));
			anim.IsDecoration = true;
			anim.Play(info.Sequence);
			rs.Add("harvest_{0}".F(info.Sequence), new AnimationWithOffset(anim,
				() => body.LocalToWorld(info.Offset.Rotate(body.QuantizeOrientation(self, self.Orientation))),
				() => !visible,
				() => false,
				p => ZOffsetFromCenter(self, p, 0)), info.Palette);
		}

		public void Harvested(Actor self, ResourceType resource)
		{
			if (visible)
				return;

			visible = true;
			anim.PlayThen(info.Sequence, () => visible = false);
		}

		public void MovingToResources(Actor self, CPos targetCell, Activity next) { }
		public void MovingToRefinery(Actor self, CPos targetCell, Activity next) { }
		public void MovementCancelled(Actor self) { }

		public static int ZOffsetFromCenter(Actor self, WPos pos, int offset)
		{
			var delta = self.CenterPosition - pos;
			return delta.Y + delta.Z + offset;
		}
	}
}
