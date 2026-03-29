// Application/Services/Repositories/IPersonalExpenseRepository.cs
using Domain.Entities;

public interface IPersonalExpenseRepository
{
    Task<PersonalExpense> AddAsync(PersonalExpense entity);
    Task DeleteAsync(PersonalExpense entity);  // eklendi
    
    Task SaveChangesAsync();  // eklendi

}
