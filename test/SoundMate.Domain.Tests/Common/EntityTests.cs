using Shouldly;
using SoundMate.Domain.Common;

namespace SoundMate.Domain.Tests.Common;

public class EntityTests
{
    private sealed class SampleEntity : Entity<Guid>
    {
        public SampleEntity(Guid id) : base(id) { }
    }

    private sealed class OtherEntity : Entity<Guid>
    {
        public OtherEntity(Guid id) : base(id) { }
    }

    [Fact]
    public void SameType_SameId_AreEqual()
    {
        var id = Guid.NewGuid();
        new SampleEntity(id).ShouldBe(new SampleEntity(id));
    }

    [Fact]
    public void SameType_DifferentId_AreNotEqual()
        => new SampleEntity(Guid.NewGuid()).ShouldNotBe(new SampleEntity(Guid.NewGuid()));

    [Fact]
    public void DifferentType_SameId_AreNotEqual()
    {
        var id = Guid.NewGuid();
        new SampleEntity(id).Equals(new OtherEntity(id)).ShouldBeFalse();
    }

    [Fact]
    public void SameId_ShareHashCode()
    {
        var id = Guid.NewGuid();
        new SampleEntity(id).GetHashCode().ShouldBe(new SampleEntity(id).GetHashCode());
    }

    [Fact]
    public void EqualityOperators_Work()
    {
        var id = Guid.NewGuid();
        var a = new SampleEntity(id);
        var b = new SampleEntity(id);
        (a == b).ShouldBeTrue();
        (a != new SampleEntity(Guid.NewGuid())).ShouldBeTrue();
    }
}
