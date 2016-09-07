// automatically generated by the FlatBuffers compiler, do not modify

namespace Iris.Serialization.Raft
{

using System;
using FlatBuffers;

public sealed class DoubleSliceFB : Table {
  public static DoubleSliceFB GetRootAsDoubleSliceFB(ByteBuffer _bb) { return GetRootAsDoubleSliceFB(_bb, new DoubleSliceFB()); }
  public static DoubleSliceFB GetRootAsDoubleSliceFB(ByteBuffer _bb, DoubleSliceFB obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public DoubleSliceFB __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public ulong Index { get { int o = __offset(4); return o != 0 ? bb.GetUlong(o + bb_pos) : (ulong)0; } }
  public double Value { get { int o = __offset(6); return o != 0 ? bb.GetDouble(o + bb_pos) : (double)0.0; } }

  public static Offset<DoubleSliceFB> CreateDoubleSliceFB(FlatBufferBuilder builder,
      ulong Index = 0,
      double Value = 0.0) {
    builder.StartObject(2);
    DoubleSliceFB.AddValue(builder, Value);
    DoubleSliceFB.AddIndex(builder, Index);
    return DoubleSliceFB.EndDoubleSliceFB(builder);
  }

  public static void StartDoubleSliceFB(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddIndex(FlatBufferBuilder builder, ulong Index) { builder.AddUlong(0, Index, 0); }
  public static void AddValue(FlatBufferBuilder builder, double Value) { builder.AddDouble(1, Value, 0.0); }
  public static Offset<DoubleSliceFB> EndDoubleSliceFB(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<DoubleSliceFB>(o);
  }
};


}
