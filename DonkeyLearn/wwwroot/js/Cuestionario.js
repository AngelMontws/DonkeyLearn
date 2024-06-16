// Inicializar el contador de preguntas
let questionCount = 1; // Iniciando desde 1 porque ya hay una pregunta en el formulario inicial

document.addEventListener("DOMContentLoaded", function () {
    updateDeleteButtonState();
});

function addQuestion() {
    var questionIndex = questionCount;
    var questionHtml = `
        <div class="question">
            <label for="pregunta${questionIndex + 1}">Pregunta ${questionIndex + 1}:</label>
            <input type="text" id="pregunta${questionIndex + 1}" name="Preguntas[${questionIndex}].Pregunta" required />
            ${getAnswersHtml(questionIndex)}
        </div>
    `;
    var form = document.querySelector('form');
    var buttonContainer = form.querySelector('.button-container');
    buttonContainer.insertAdjacentHTML('beforebegin', questionHtml);
    questionCount++;
    updateDeleteButtonState();
}

function getAnswersHtml(questionIndex) {
    var answersHtml = '';
    for (var i = 0; i < 4; i++) {
        var answerIndex = i + 1;
        answersHtml += `
            <div class="answer">
                <label for="inciso${questionIndex + 1}-${answerIndex}">Inciso ${answerIndex}:</label>
                <input type="text" id="inciso${questionIndex + 1}-${answerIndex}" name="Preguntas[${questionIndex}].Respuestas[${i}].Respuesta" required />
                <input type="checkbox" id="correcta${questionIndex + 1}-${answerIndex}" name="Preguntas[${questionIndex}].Respuestas[${i}].EsCorrecta" value="true" />
                <label for="correcta${questionIndex + 1}-${answerIndex}">Correcta</label>
            </div>
        `;
    }
    return answersHtml;
}

function deleteQuestion() {
    var questionContainers = document.querySelectorAll('form > .question');
    if (questionContainers.length > 1) {
        var lastQuestionContainer = questionContainers[questionContainers.length - 1];
        lastQuestionContainer.remove();
        questionCount--;
    } else {
        alert("No se puede eliminar la última pregunta.");
    }
    updateDeleteButtonState();
}

function updateDeleteButtonState() {
    var questionContainers = document.querySelectorAll('form > .question');
    var deleteButton = document.getElementById('deleteQuestionButton');
    if (questionContainers.length > 1) {
        deleteButton.disabled = false;
    } else {
        deleteButton.disabled = true;
    }
}
