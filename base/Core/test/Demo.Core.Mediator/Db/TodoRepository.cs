using System.Collections.Generic;
using System.Linq;
using Demo.Core.Mediator.Models;

namespace Demo.Core.Mediator.Db
{
    internal class TodoRepository
    {
        private int nextId = 1;

        private List<Todo> data = new List<Todo>();

        public int Add(Todo model)
        {
            model = new Todo(nextId++, model.Value);
            data.Add(model);

            return model.Id;
        }

        public bool Delete(int id)
        {
            var item = data.FirstOrDefault(e => e.Id == id);
            if (item is null)
                return false;

            data.Remove(item);

            return true;
        }

        public IEnumerable<Todo> GetAll() => data.AsEnumerable();
    }
}