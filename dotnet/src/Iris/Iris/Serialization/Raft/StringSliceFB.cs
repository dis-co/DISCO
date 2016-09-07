// automatically generated by the FlatBuffers compiler, do not modify

namespace Iris.Serialization.Raft
{

using System;
using FlatBuffers;

public sealed class StringSliceFB : Table {
  public static StringSliceFB GetRootAsStringSliceFB(ByteBuffer _bb) { return GetRootAsStringSliceFB(_bb, new StringSliceFB()); }
  public static StringSliceFB GetRootAsStringSliceFB(ByteBuffer _bb, StringSliceFB obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public StringSliceFB __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public ulong Index { get { int o = __offset(4); return o != 0 ? bb.GetUlong(o + bb_pos) : (ulong)0; } }
  public string Value { get { int o = __offset(6); return o != 0 ? __string(o + bb_pos) : null; } }
  public ArraySegment<byte>? GetValueBytes() { return __vector_as_arraysegment(6); }

  public static Offset<StringSliceFB> CreateStringSliceFB(FlatBufferBuilder builder,
      ulong Index = 0,
      StringOffset ValueOffset = default(StringOffset)) {
    builder.StartObject(2);
    StringSliceFB.AddIndex(builder, Index);
    StringSliceFB.AddValue(builder, ValueOffset);
    return StringSliceFB.EndStringSliceFB(builder);
  }

  public static void StartStringSliceFB(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddIndex(FlatBufferBuilder builder, ulong Index) { builder.AddUlong(0, Index, 0); }
  public static void AddValue(FlatBufferBuilder builder, StringOffset ValueOffset) { builder.AddOffset(1, ValueOffset.Value, 0); }
  public static Offset<StringSliceFB> EndStringSliceFB(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<StringSliceFB>(o);
  }
};


}
