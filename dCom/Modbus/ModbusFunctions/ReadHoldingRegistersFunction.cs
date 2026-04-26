using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read holding registers functions/requests.
    /// </summary>
    public class ReadHoldingRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadHoldingRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters parameters = (ModbusReadCommandParameters)CommandParameters;

            byte[] request = new byte[12];

            request[0] = (byte)(parameters.TransactionId >> 8);
            request[1] = (byte)(parameters.TransactionId & 0xFF);

            request[2] = 0;
            request[3] = 0;

            request[4] = (byte)(parameters.Length >> 8);
            request[5] = (byte)(parameters.Length & 0xFF);

            request[6] = parameters.UnitId;
            request[7] = parameters.FunctionCode;

            request[8] = (byte)(parameters.StartAddress >> 8);
            request[9] = (byte)(parameters.StartAddress & 0xFF);

            request[10] = (byte)(parameters.Quantity >> 8);
            request[11] = (byte)(parameters.Quantity & 0xFF);

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> result = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusReadCommandParameters parameters = (ModbusReadCommandParameters)CommandParameters;

            byte functionCode = response[7];

            if ((functionCode & 0x80) != 0)
            {
                HandeException(response[8]);
            }

            ushort startAddress = parameters.StartAddress;
            ushort quantity = parameters.Quantity;

            for (int i = 0; i < quantity; i++)
            {
                int dataIndex = 9 + (i * 2);
                ushort value = (ushort)((response[dataIndex] << 8) | response[dataIndex + 1]);

                result.Add(
                    new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, (ushort)(startAddress + i)),
                    value);
            }

            return result;
        }
    }
}