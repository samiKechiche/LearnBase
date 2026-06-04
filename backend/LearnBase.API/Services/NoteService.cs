using LearnBase.API.Data;
using LearnBase.API.DTOs.Note;
using LearnBase.API.Models;
using Microsoft.EntityFrameworkCore;
using System;

using DocumentFormat.OpenXml.Packaging;

namespace LearnBase.API.Services
{
    public class NoteService
    {
        private readonly ApplicationDbContext _context;

        public NoteService(ApplicationDbContext context)
        {
            _context = context;
        }

        // CREATE
        public async Task<NoteResponseDto> CreateNote(CreateNoteDto dto)
        {
            var note = new Note
            {
                Content = dto.Content,
                LessonId = dto.LessonId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            return new NoteResponseDto
            {
                NoteId = note.NoteId,
                Content = note.Content,
                CreatedAt = note.CreatedAt
            };
        }
        // CREATE FROM FILE
        public async Task<Note> CreateFromDocxAsync(IFormFile file, Guid lessonId)
        {
            string text;

            using (var stream = file.OpenReadStream())
            using (var doc = WordprocessingDocument.Open(stream, false))
            {
                text = doc.MainDocumentPart?.Document?.Body?.InnerText ?? "";
            }

            var note = new Note
            {
                Content = text,
                LessonId = lessonId
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            return note;
        }

        // READ (single)
        public async Task<NoteResponseDto?> GetNote(Guid id)
        {
            var note = await _context.Notes.FindAsync(id);

            if (note == null) return null;

            return new NoteResponseDto
            {
                NoteId = note.NoteId,
                Content = note.Content,
                CreatedAt = note.CreatedAt
            };
        }

        // READ (all notes for a lesson) ← VERY useful for you
        public async Task<List<NoteResponseDto>> GetNotesByLesson(Guid lessonId)
        {
            return await _context.Notes
                .Where(n => n.LessonId == lessonId)
                .Select(n => new NoteResponseDto
                {
                    NoteId = n.NoteId,
                    Content = n.Content,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        // UPDATE
        public async Task<bool> UpdateNote(Guid id, UpdateNoteDto dto)
        {
            var note = await _context.Notes.FindAsync(id);

            if (note == null) return false;

            note.Content = dto.Content;
            note.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // DELETE
        public async Task<bool> DeleteNote(Guid id)
        {
            var note = await _context.Notes.FindAsync(id);

            if (note == null) return false;

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<List<Note>> GetByLessonIdAsync(Guid lessonId)
        {
            return await _context.Notes
                .Where(n => n.LessonId == lessonId)
                .ToListAsync();
        }
    }
}