﻿namespace Radar7D.Models.Dtos
{
    public class SendEncryptDto
    {
        public SendEncryptDto(string data)
        {
            Data = data;
        }
        public string Data { get; set; }
    }
}
