function addQuestion() {
    var questionIndex = questionCount;
    var questionHtml = `
                <div>
                    <label for="pregunta">Pregunta:</label>
                    <input type="text" id="pregunta" name="Preguntas[${questionIndex}].Pregunta" required />
                </div>
            `;
    var answersHtml = '';
    for (var i = 0; i < 4; i++) {
        var answerIndex = i + 1;
        answersHtml += `
                    <div>
                        <label for="inciso${answerIndex}">Inciso ${answerIndex}:</label>
                        <input type="text" id="inciso${answerIndex}" name="Preguntas[${questionIndex}].Respuestas[${i}].Respuesta" required />
                        <input type="checkbox" id="correcta${answerIndex}" name="Preguntas[${questionIndex}].Respuestas[${i}].EsCorrecta" value="true" />
                        <label for="correcta${answerIndex}">Correcta</label>
                    </div>
                `;
    }
    var questionContainer = document.createElement('div');
    questionContainer.innerHTML = questionHtml + answersHtml;
    var form = document.querySelector('form');
    form.insertBefore(questionContainer, form.querySelector('button[type="button"]'));
    updateDeleteButtonState();
    questionCount++;
}
function deleteQuestion() {
    var questionContainers = document.querySelectorAll('form > div:not(:first-child)');
    if (questionContainers.length > 1) {
        var lastQuestionContainer = questionContainers[questionContainers.length - 1];
        lastQuestionContainer.remove();
        for (var i = 0; i < questionCount; i++) {
            var questionContainer = questionContainers[i];
            questionContainer.querySelector('input[name^="Preguntas["]').name = `Preguntas[${i}].Pregunta`;
            var answerInputs = questionContainer.querySelectorAll('input[name^="Preguntas[' + (i + 1) + '].Respuestas["]');
            for (var j = 0; j < answerInputs.length; j++) {
                answerInputs[j].name = `Preguntas[${i}].Respuestas[${j}].Respuesta`;
            }
        }
    } else {
        alert("No se puede eliminar la última pregunta.");
    }
    updateDeleteButtonState();
}

function updateDeleteButtonState() {
    var questionContainers = document.querySelectorAll('form > div:not(:first-child)');
    var deleteButton = document.getElementById('deleteQuestionButton');
    if (questionContainers.length > 3) {
        deleteButton.disabled = false;
    } else {
        deleteButton.disabled = true;
    }
}
