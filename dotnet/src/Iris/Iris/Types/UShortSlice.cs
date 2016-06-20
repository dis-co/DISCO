// automatically generated by the FlatBuffers compiler, do not modify

namespace Iris.Types
{

using System;
using FlatBuffers;

public sealed class UShortSlice : Table {
  public static UShortSlice GetRootAsUShortSlice(ByteBuffer _bb) { return GetRootAsUShortSlice(_bb, new UShortSlice()); }
  public static UShortSlice GetRootAsUShortSlice(ByteBuffer _bb, UShortSlice obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public UShortSlice __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public byte Index { get { int o = __offset(4); return o != 0 ? bb.Get(o + bb_pos) : (byte)0; } }
  public ushort Value { get { int o = __offset(6); return o != 0 ? bb.GetUshort(o + bb_pos) : (ushort)0; } }

  public static Offset<UShortSlice> CreateUShortSlice(FlatBufferBuilder builder,
      byte Index = 0,
      ushort Value = 0) {
    builder.StartObject(2);
    UShortSlice.AddValue(builder, Value);
    UShortSlice.AddIndex(builder, Index);
    return UShortSlice.EndUShortSlice(builder);
  }

  public static void StartUShortSlice(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddIndex(FlatBufferBuilder builder, byte Index) { builder.AddByte(0, Index, 0); }
  public static void AddValue(FlatBufferBuilder builder, ushort Value) { builder.AddUshort(1, Value, 0); }
  public static Offset<UShortSlice> EndUShortSlice(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<UShortSlice>(o);
  }
};


}