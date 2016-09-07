// automatically generated by the FlatBuffers compiler, do not modify

namespace Iris.Serialization.Raft
{

using System;
using FlatBuffers;

public sealed class FloatSliceFB : Table {
  public static FloatSliceFB GetRootAsFloatSliceFB(ByteBuffer _bb) { return GetRootAsFloatSliceFB(_bb, new FloatSliceFB()); }
  public static FloatSliceFB GetRootAsFloatSliceFB(ByteBuffer _bb, FloatSliceFB obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public FloatSliceFB __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public ulong Index { get { int o = __offset(4); return o != 0 ? bb.GetUlong(o + bb_pos) : (ulong)0; } }
  public float Value { get { int o = __offset(6); return o != 0 ? bb.GetFloat(o + bb_pos) : (float)0.0f; } }

  public static Offset<FloatSliceFB> CreateFloatSliceFB(FlatBufferBuilder builder,
      ulong Index = 0,
      float Value = 0.0f) {
    builder.StartObject(2);
    FloatSliceFB.AddIndex(builder, Index);
    FloatSliceFB.AddValue(builder, Value);
    return FloatSliceFB.EndFloatSliceFB(builder);
  }

  public static void StartFloatSliceFB(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddIndex(FlatBufferBuilder builder, ulong Index) { builder.AddUlong(0, Index, 0); }
  public static void AddValue(FlatBufferBuilder builder, float Value) { builder.AddFloat(1, Value, 0.0f); }
  public static Offset<FloatSliceFB> EndFloatSliceFB(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<FloatSliceFB>(o);
  }
};


}
