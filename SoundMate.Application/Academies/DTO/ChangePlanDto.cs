using SoundMate.Domain.Academies;

namespace SoundMate.Application.Academies.DTO;

/// <summary>
/// Moves an academy to another subscription plan.
/// <para>
/// A plain value change, on purpose. <c>SubscriptionPlan</c> still carries an unanswered question
/// in the domain — what each tier actually includes — so nothing here enforces limits, prorates
/// anything or talks to a payment provider. Building those rules before that conversation happens
/// would mean guessing at them.
/// </para>
/// </summary>
public sealed record ChangePlanDto
{
    /// <summary><c>Free</c>, <c>Basic</c> or <c>Pro</c>. Accepted by name or by number.</summary>
    public SubscriptionPlan Plan { get; init; }
}
