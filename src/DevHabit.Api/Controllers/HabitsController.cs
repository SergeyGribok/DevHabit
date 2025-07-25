using System.Linq.Expressions;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("habits")]
public sealed class HabitsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public HabitsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<HabitsCollectionDto>> GetHabits()
    {
        List<HabitDto> habits = await _dbContext
            .Habits
            .Select(HabitQueries.ProjectToDto())
            .ToListAsync();

        var habitsCollection = new HabitsCollectionDto
        {
            Data = habits
        };

        return Ok(habitsCollection);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HabitWithTagsDto>> GetHabit(string id)
    {
        HabitWithTagsDto? habit = await _dbContext
            .Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToDtoWithTags())
            .FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound();
        }

        return Ok(habit);
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(
        CreateHabitDto createHabitDto,
        IValidator<CreateHabitDto> validator)
    {

        ValidationResult validationResult = await validator.ValidateAsync(createHabitDto);

        if(!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var habit = createHabitDto.ToEntity();

        _dbContext.Habits.Add(habit);
        await _dbContext.SaveChangesAsync();

        var habitDto = habit.ToDto();

        return CreatedAtAction(nameof(GetHabit), new {id = habitDto.Id}, habitDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, UpdateHabitDto updateHabitDto)
    {
        Habit? habit = await _dbContext.Habits
            .FirstOrDefaultAsync(h => h.Id == id);

        if(habit is null)
        {
            return NotFound();
        }

        habit.UpdateFromDto(updateHabitDto);

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch]
    public async Task<ActionResult> PatchHabit(string id, JsonPatchDocument<HabitDto> patchDocument)
    {
        Habit? habit = await _dbContext.Habits
          .FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        HabitDto habitDto = habit.ToDto();

        patchDocument.ApplyTo(habitDto);

        if(!TryValidateModel(habitDto))
        {
            return ValidationProblem(ModelState);
        }

        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return NoContent();

    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id)
    {
        Habit? habit = await _dbContext.Habits
            .FirstOrDefaultAsync(h => h.Id == id);

        if (habit is null)
        {
            return NotFound();
        }

        _dbContext.Habits.Remove(habit);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
