using LinqTasks;

BusinessLogic businessLogic = new();

foreach (var item in businessLogic.GetUsersPage(-1, 1))
{
    Console.WriteLine(item.ToString());
}