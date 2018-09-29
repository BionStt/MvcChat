using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MvcChat.Model;
using MvcChat.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace MvcChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : Controller
    {
        private const int _msgTimeout = 60000;

        public ChatController(IClientRepository clientRepository, IMessageService msgService)
        {
            _clientRepository = clientRepository;
            _msgService = msgService;
        }

        private IClientRepository _clientRepository;
        private IMessageService _msgService;

        [HttpGet("[action]/{id}")]
        public async Task< IActionResult > Listen([Required, RegularExpression(@"\d{1,64}")] string id) {

            if(!_clientRepository.Has(id))
                return null;

            return Ok(await _msgService.GetMessagesFor(id, _msgTimeout));
        }

        [HttpPost("[action]")]
        public bool Send([FromBody, Required] Message msg) {

            if(msg.to_id!=null && msg.to_id.Trim().Length>1 && !_clientRepository.Has(msg.to_id))
                return false;

            return _msgService.AddMessage(msg);
        }

        [HttpPost("[action]")]
        public Object Join([FromBody, Required, RegularExpression(@"\S{1,64}")] string name) {

            string id = _clientRepository.Add(name);
            if(null!=id)
            {
                _msgService.Init(id);

                Message msg_info = new Message{type = MessageType.TextMessage, from_id="", to_id="", data=new { text = $"{name} ({id}) has joined to the chat."}};
                _msgService.AddMessage(msg_info);

                Message msg = new Message{type = MessageType.UsersList, from_id="", to_id="", data=_clientRepository.Clients.ToArray()};
                _msgService.AddMessage(msg);

                return id as Object;
            }

            return string.Empty as Object;
        }

        [HttpPost("[action]")]
        public void Leave([FromBody, Required, RegularExpression(@"\d{1,64}")] string id) {

            Client c = _clientRepository.Delete(id);

            Message msg_info = new Message{type = MessageType.TextMessage, from_id="", to_id="", data=new { text = $"{c?.name} ({id}) has left the chat."}};
            _msgService.AddMessage(msg_info);

            _msgService.Free(id);
            
            Message msg = new Message{type = MessageType.UsersList, from_id="", to_id="", data=_clientRepository.Clients.ToArray()};
            _msgService.AddMessage(msg);
        }
    }
}
