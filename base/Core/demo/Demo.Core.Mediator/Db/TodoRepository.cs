using System.Collections.Generic;
using System.Linq;
using Demo.Core.Mediator.Models;

namespace Demo.Core.Mediator.Db;

internal class TodoRepository
{
    private int _nextId = 1;

    private readonly List<Todo> _data = new();

    public int Add(Todo model)
    {
        model = new Todo(_nextId++, model.Value);
        _data.Add(model);

        return model.Id;
    }

    public bool Delete(int id)
    {
        var item = _data.FirstOrDefault(e => e.Id == id);
        if (item is null)
            return false;

        _data.Remove(item);

        return true;
    }

    public IEnumerable<Todo> GetAll() => _data.AsEnumerable();
}